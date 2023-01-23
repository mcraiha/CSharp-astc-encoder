// SPDX-License-Identifier: Apache-2.0
// ----------------------------------------------------------------------------
// Copyright 2011-2021 Arm Limited
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License. You may obtain a copy
// of the License at:
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations
// under the License.
// ----------------------------------------------------------------------------

/**
 * @brief Soft-float library for IEEE-754.
 */
#if TRUE //(ASTCENC_F16C == 0) && (ASTCENC_NEON == 0)


/*	sized soft-float types. These are mapped to the sized integer
    types of C99, instead of C's floating-point types; this is because
    the library needs to maintain exact, bit-level control on all
    operations on these data types. */
global using sf16 = System.UInt16;
global using sf32 = System.UInt32;

namespace ASTCEnc
{
    public static class ASTCMathSoftFloat
    {
        /******************************************
        helper functions and their lookup tables
        ******************************************/
        /* count leading zeros functions. Only used when the input is nonzero. */


        /* table used for the slow default versions. */
        public static readonly byte[] clz_table = new byte[256]
        {
            8, 7, 6, 6, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, 4, 4,
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        /*
        32-bit count-leading-zeros function: use the Assembly instruction whenever possible. */
        static uint clz32(uint inp)
        {
            
            /* slow default version */
            uint summa = 24;
            if (inp >= 0x10000)
            {
                inp >>= 16;
                summa -= 16;
            }
            if (inp >= 0x100)
            {
                inp >>= 8;
                summa -= 8;
            }
            return summa + clz_table[inp];
        }

        /* the five rounding modes that IEEE-754r defines */
        enum roundmode
        {
            SF_UP = 0,				/* round towards positive infinity */
            SF_DOWN = 1,			/* round towards negative infinity */
            SF_TOZERO = 2,			/* round towards zero */
            SF_NEARESTEVEN = 3,		/* round toward nearest value; if mid-between, round to even value */
            SF_NEARESTAWAY = 4		/* round toward nearest value; if mid-between, round away from zero */
        }


        static uint rtne_shift32(uint inp, uint shamt)
        {
            uint vl1 = 1 << shamt;
            uint inp2 = inp + (vl1 >> 1);	/* added 0.5 ULP */
            uint msk = (inp | 1) & vl1;	/* nonzero if odd. '| 1' forces it to 1 if the shamt is 0. */
            msk--;						/* negative if even, nonnegative if odd. */
            inp2 -= (msk >> 31);		/* subtract epsilon before shift if even. */
            inp2 >>= shamt;
            return inp2;
        }

        static uint rtna_shift32(uint inp, uint shamt)
        {
            uint vl1 = (1 << shamt) >> 1;
            inp += vl1;
            inp >>= shamt;
            return inp;
        }

        static uint rtup_shift32(uint inp, uint shamt)
        {
            uint vl1 = 1 << shamt;
            inp += vl1;
            inp--;
            inp >>= shamt;
            return inp;
        }

        static uint WITH_MSB(uint a)
        {
            return a | (1u << 31);
        }

        /* convert from FP16 to FP32. */
        static sf32 sf16_to_sf32(sf16 inp)
        {
            uint inpx = inp;

            /*
                This table contains, for every FP16 sign/exponent value combination,
                the difference between the input FP16 value and the value obtained
                by shifting the correct FP32 result right by 13 bits.
                This table allows us to handle every case except denormals and NaN
                with just 1 table lookup, 2 shifts and 1 add.
            */


            uint[] tbl = new uint[64]
            {
                WITH_MSB(0x00000), 0x1C000, 0x1C000, 0x1C000, 0x1C000, 0x1C000, 0x1C000,          0x1C000,
                        0x1C000,  0x1C000, 0x1C000, 0x1C000, 0x1C000, 0x1C000, 0x1C000,          0x1C000,
                        0x1C000,  0x1C000, 0x1C000, 0x1C000, 0x1C000, 0x1C000, 0x1C000,          0x1C000,
                        0x1C000,  0x1C000, 0x1C000, 0x1C000, 0x1C000, 0x1C000, 0x1C000, WITH_MSB(0x38000),
                WITH_MSB(0x38000), 0x54000, 0x54000, 0x54000, 0x54000, 0x54000, 0x54000,          0x54000,
                        0x54000,  0x54000, 0x54000, 0x54000, 0x54000, 0x54000, 0x54000,          0x54000,
                        0x54000,  0x54000, 0x54000, 0x54000, 0x54000, 0x54000, 0x54000,          0x54000,
                        0x54000,  0x54000, 0x54000, 0x54000, 0x54000, 0x54000, 0x54000, WITH_MSB(0x70000)
            };

            uint res = tbl[inpx >> 10];
            res += inpx;

            /* Normal cases: MSB of 'res' not set. */
            if ((res & WITH_MSB(0)) == 0)
            {
                return res << 13;
            }

            /* Infinity and Zero: 10 LSB of 'res' not set. */
            if ((res & 0x3FF) == 0)
            {
                return res << 13;
            }

            /* NaN: the exponent field of 'inp' is non-zero. */
            if ((inpx & 0x7C00) != 0)
            {
                /* All NaNs are quietened. */
                return (res << 13) | 0x400000;
            }

            /* Denormal cases */
            uint sign = (inpx & 0x8000) << 16;
            uint mskval = inpx & 0x7FFF;
            uint leadingzeroes = clz32(mskval);
            mskval <<= leadingzeroes;
            return (mskval >> 8) + ((0x85 - leadingzeroes) << 23) + sign;
        }

        /* Conversion routine that converts from FP32 to FP16. It supports denormals and all rounding modes. If a NaN is given as input, it is quietened. */
        static sf16 sf32_to_sf16(sf32 inp, roundmode rmode)
        {
            /* for each possible sign/exponent combination, store a case index. This gives a 512-byte table */
            byte[] tab = new byte[512] {
                0, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
                10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
                10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
                10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
                10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
                10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
                10, 10, 10, 10, 10, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
                20, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30,
                30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 40,
                40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40,
                40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40,
                40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40,
                40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40,
                40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40,
                40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40,
                40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 50,

                5, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
                15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
                15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
                15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
                15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
                15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
                15, 15, 15, 15, 15, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
                25, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35,
                35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 45,
                45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
                45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
                45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
                45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
                45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
                45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
                45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 55,
            };

            /* many of the cases below use a case-dependent magic constant. So we look up a magic constant before actually performing the switch. This table allows us to group cases, thereby minimizing code
            size. */
            uint[] tabx = new uint[60] {
                0, 0, 0, 0, 0, 0x8000, 0x80000000, 0x8000, 0x8000, 0x8000,
                1, 0, 0, 0, 0, 0x8000, 0x8001, 0x8000, 0x8000, 0x8000,
                0, 0, 0, 0, 0, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
                0xC8001FFF, 0xC8000000, 0xC8000000, 0xC8000FFF, 0xC8001000,
                0x58000000, 0x38001FFF, 0x58000000, 0x58000FFF, 0x58001000,
                0x7C00, 0x7BFF, 0x7BFF, 0x7C00, 0x7C00,
                0xFBFF, 0xFC00, 0xFBFF, 0xFC00, 0xFC00,
                0x90000000, 0x90000000, 0x90000000, 0x90000000, 0x90000000,
                0x20000000, 0x20000000, 0x20000000, 0x20000000, 0x20000000
            };

            uint p;
            uint idx = rmode + tab[inp >> 23];
            uint vlx = tabx[idx];
            switch (idx)
            {
                /*
                    Positive number which may be Infinity or NaN.
                    We need to check whether it is NaN; if it is, quieten it by setting the top bit of the mantissa.
                    (If we don't do this quieting, then a NaN  that is distinguished only by having
                    its low-order bits set, would be turned into an INF. */
            case 50:
            case 51:
            case 52:
            case 53:
            case 54:
            case 55:
            case 56:
            case 57:
            case 58:
            case 59:
                /*
                    the input value is 0x7F800000 or 0xFF800000 if it is INF.
                    By subtracting 1, we get 7F7FFFFF or FF7FFFFF, that is, bit 23 becomes zero.
                    For NaNs, however, this operation will keep bit 23 with the value 1.
                    We can then extract bit 23, and logical-OR bit 9 of the result with this
                    bit in order to quieten the NaN (a Quiet NaN is a NaN where the top bit
                    of the mantissa is set.)
                */
                p = (inp - 1) & 0x800000;	/* zero if INF, nonzero if NaN. */
                return (sf16)(((inp + vlx) >> 13) | (p >> 14));
                /*
                    positive, exponent = 0, round-mode == UP; need to check whether number actually is 0.
                    If it is, then return 0, else return 1 (the smallest representable nonzero number)
                */
            case 0:
                /*
                    -inp will set the MSB if the input number is nonzero.
                    Thus (-inp) >> 31 will turn into 0 if the input number is 0 and 1 otherwise.
                */
                return (sf16)(uint)((-(int)(inp))) >> 31);

                /*
                    negative, exponent = , round-mode == DOWN, need to check whether number is
                    actually 0. If it is, return 0x8000 ( float -0.0 )
                    Else return the smallest negative number ( 0x8001 ) */
            case 6:
                /*
                    in this case 'vlx' is 0x80000000. By subtracting the input value from it,
                    we obtain a value that is 0 if the input value is in fact zero and has
                    the MSB set if it isn't. We then right-shift the value by 31 places to
                    get a value that is 0 if the input is -0.0 and 1 otherwise.
                */
                return (sf16)(((vlx - inp) >> 31) + 0x8000);

                /*
                    for all other cases involving underflow/overflow, we don't need to
                    do actual tests; we just return 'vlx'.
                */
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
            case 18:
            case 19:
            case 40:
            case 41:
            case 42:
            case 43:
            case 44:
            case 45:
            case 46:
            case 47:
            case 48:
            case 49:
                return (sf16)(vlx);

                /*
                    for normal numbers, 'vlx' is the difference between the FP32 value of a number and the
                    FP16 representation of the same number left-shifted by 13 places. In addition, a rounding constant is
                    baked into 'vlx': for rounding-away-from zero, the constant is 2^13 - 1, causing roundoff away
                    from zero. for round-to-nearest away, the constant is 2^12, causing roundoff away from zero.
                    for round-to-nearest-even, the constant is 2^12 - 1. This causes correct round-to-nearest-even
                    except for odd input numbers. For odd input numbers, we need to add 1 to the constant. */

                /* normal number, all rounding modes except round-to-nearest-even: */
            case 30:
            case 31:
            case 32:
            case 34:
            case 35:
            case 36:
            case 37:
            case 39:
                return (sf16)((inp + vlx) >> 13);

                /* normal number, round-to-nearest-even. */
            case 33:
            case 38:
                p = inp + vlx;
                p += (inp >> 13) & 1;
                return (sf16)(p >> 13);

                /*
                    the various denormal cases. These are not expected to be common, so their performance is a bit
                    less important. For each of these cases, we need to extract an exponent and a mantissa
                    (including the implicit '1'!), and then right-shift the mantissa by a shift-amount that
                    depends on the exponent. The shift must apply the correct rounding mode. 'vlx' is used to supply the
                    sign of the resulting denormal number.
                */
            case 21:
            case 22:
            case 25:
            case 27:
                /* denormal, round towards zero. */
                p = 126 - ((inp >> 23) & 0xFF);
                return (sf16)((((inp & 0x7FFFFF) + 0x800000) >> p) | vlx);
            case 20:
            case 26:
                /* denormal, round away from zero. */
                p = 126 - ((inp >> 23) & 0xFF);
                return (sf16)(rtup_shift32((inp & 0x7FFFFF) + 0x800000, p) | vlx);
            case 24:
            case 29:
                /* denormal, round to nearest-away */
                p = 126 - ((inp >> 23) & 0xFF);
                return (sf16)(rtna_shift32((inp & 0x7FFFFF) + 0x800000, p) | vlx);
            case 23:
            case 28:
                /* denormal, round to nearest-even. */
                p = 126 - ((inp >> 23) & 0xFF);
                return (sf16)(rtne_shift32((inp & 0x7FFFFF) + 0x800000, p) | vlx);
            }

            return 0;
        }

        /* convert from soft-float to native-float */
        float sf16_to_float(ushort p)
        {
            if32 i;
            i.u = sf16_to_sf32(p);
            return i.f;
        }

        /* convert from native-float to soft-float */
        ushort float_to_sf16(float p)
        {
            if32 i;
            i.f = p;
            return sf32_to_sf16(i.u, SF_NEARESTEVEN);
        }
    }
}
#endif