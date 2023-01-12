using System.Threading;

namespace ASTCEnc
{
    /* ============================================================================
    Parallel execution control
    ============================================================================ */

    /**
    * @brief A simple counter-based manager for parallel task execution.
    *
    * The task processing execution consists of:
    *
    *     * A single-threaded init stage.
    *     * A multi-threaded processing stage.
    *     * A condition variable so threads can wait for processing completion.
    *
    * The init stage will be executed by the first thread to arrive in the critical section, there is
    * no main thread in the thread pool.
    *
    * The processing stage uses dynamic dispatch to assign task tickets to threads on an on-demand
    * basis. Threads may each therefore executed different numbers of tasks, depending on their
    * processing complexity. The task queue and the task tickets are just counters; the caller must map
    * these integers to an actual processing partition in a specific problem domain.
    *
    * The exit wait condition is needed to ensure processing has finished before a worker thread can
    * progress to the next stage of the pipeline. Specifically a worker may exit the processing stage
    * because there are no new tasks to assign to it while other worker threads are still processing.
    * Calling @c wait() will ensure that all other worker have finished before the thread can proceed.
    *
    * The basic usage model:
    *
    *     // --------- From single-threaded code ---------
    *
    *     // Reset the tracker state
    *     manager->reset()
    *
    *     // --------- From multi-threaded code ---------
    *
    *     // Run the stage init; only first thread actually runs the lambda
    *     manager->init(<lambda>)
    *
    *     do
    *     {
    *         // Request a task assignment
    *         uint task_count;
    *         uint base_index = manager->get_tasks(<granule>, task_count);
    *
    *         // Process any tasks we were given (task_count <= granule size)
    *         if (task_count)
    *         {
    *             // Run the user task processing code for N tasks here
    *             ...
    *
    *             // Flag these tasks as complete
    *             manager->complete_tasks(task_count);
    *         }
    *     } while (task_count);
    *
    *     // Wait for all threads to complete tasks before progressing
    *     manager->wait()
    *
    *     // Run the stage term; only first thread actually runs the lambda
    *     manager->term(<lambda>)
    */
    class ParallelManager
    {
        /** @brief Lock used for critical section and condition synchronization. */
        private Mutex m_lock;

        /** @brief True if the stage init() step has been executed. */
        private bool m_init_done;

        /** @brief True if the stage term() step has been executed. */
        private bool m_term_done;

        /** @brief Condition variable for tracking stage processing completion. */
        std::condition_variable m_complete;

        /** @brief Number of tasks started, but not necessarily finished. */
        std::atomic<uint> m_start_count;

        /** @brief Number of tasks finished. */
        private uint m_done_count;

        /** @brief Number of tasks that need to be processed. */
        private uint m_task_count;

        /** @brief Create a new ParallelManager. */
        public ParallelManager()
        {
            reset();
        }

        /**
        * @brief Reset the tracker for a new processing batch.
        *
        * This must be called from single-threaded code before starting the multi-threaded processing
        * operations.
        */
        public void reset()
        {
            m_init_done = false;
            m_term_done = false;
            m_start_count = 0;
            m_done_count = 0;
            m_task_count = 0;
        }

        /**
        * @brief Trigger the pipeline stage init step.
        *
        * This can be called from multi-threaded code. The first thread to hit this will process the
        * initialization. Other threads will block and wait for it to complete.
        *
        * @param init_func   Callable which executes the stage initialization. It must return the
        *                    total number of tasks in the stage.
        */
        public void init(std::function<uint(void)> init_func)
        {
            std::lock_guard<std::mutex> lck(m_lock);
            if (!m_init_done)
            {
                m_task_count = init_func();
                m_init_done = true;
            }
        }

        /**
        * @brief Trigger the pipeline stage init step.
        *
        * This can be called from multi-threaded code. The first thread to hit this will process the
        * initialization. Other threads will block and wait for it to complete.
        *
        * @param task_count   Total number of tasks needing processing.
        */
        public void init(uint task_count)
        {
            std::lock_guard<std::mutex> lck(m_lock);
            if (!m_init_done)
            {
                m_task_count = task_count;
                m_init_done = true;
            }
        }

        /**
        * @brief Request a task assignment.
        *
        * Assign up to @c granule tasks to the caller for processing.
        *
        * @param      granule   Maximum number of tasks that can be assigned.
        * @param[out] count     Actual number of tasks assigned, or zero if no tasks were assigned.
        *
        * @return Task index of the first assigned task; assigned tasks increment from this.
        */
        public uint get_task_assignment(uint granule, out uint count)
        {
            uint taskIndex = m_start_count.fetch_add(granule, std::memory_order_relaxed);
            if (taskIndex >= m_task_count)
            {
                count = 0;
                return 0;
            }

            count = ASTCMath.min(m_task_count - taskIndex, granule);
            return taskIndex;
        }

        /**
        * @brief Complete a task assignment.
        *
        * Mark @c count tasks as complete. This will notify all threads blocked on @c wait() if this
        * completes the processing of the stage.
        *
        * @param count   The number of completed tasks.
        */
        public void complete_task_assignment(uint count)
        {
            // Note: m_done_count cannot use an atomic without the mutex; this has a race between the
            // update here and the wait() for other threads
            std::unique_lock<std::mutex> lck(m_lock);
            this.m_done_count += count;
            if (m_done_count == m_task_count)
            {
                lck.unlock();
                m_complete.notify_all();
            }
        }

        /**
        * @brief Wait for stage processing to complete.
        */
        public void wait()
        {
            std::unique_lock<std::mutex> lck(m_lock);
            m_complete.wait(lck, [this]{ return m_done_count == m_task_count; });
        }

        /**
        * @brief Trigger the pipeline stage term step.
        *
        * This can be called from multi-threaded code. The first thread to hit this will process the
        * work pool termination. Caller must have called @c wait() prior to calling this function to
        * ensure that processing is complete.
        *
        * @param term_func   Callable which executes the stage termination.
        */
        public void term(std::function<void(void)> term_func)
        {
            std::lock_guard<std::mutex> lck(m_lock);
            if (!m_term_done)
            {
                term_func();
                m_term_done = true;
            }
        }
    }

    /**
    * @brief The astcenc compression context.
    */
    public struct astcenc_context
    {
        /** @brief The context internal state. */
        public astcenc_contexti context;

    #if !ASTCENC_DECOMPRESS_ONLY
        /** @brief The parallel manager for averages computation. */
        public ParallelManager manage_avg;

        /** @brief The parallel manager for compression. */
        public ParallelManager manage_compress;
    #endif

        /** @brief The parallel manager for decompression. */
        public ParallelManager manage_decompress;
    }
}