using UnityEngine;
using System.Collections;

namespace Threading {

////////////////////////////////////////////////////////////////////////////////
// Dataparallel, multithreaded loops using threadpools.
// Inspired by Example 3 on 
// http://msdn.microsoft.com/en-us/library/aa645740(v=vs.71).aspx#vcwlkthreadingtutorialexample3threadpool
////////////////////////////////////////////////////////////////////////////////
public static class Parallel {

    private static bool? _supported = null;
    public static bool Supported() {
        if (!_supported.HasValue) {
            try {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback( state => {}));
                _supported = true;
            } catch (System.NotSupportedException) {
                _supported = false;
            }
        }
        return _supported.Value;
    }

    private class CountingLock {
        private int countsLeft;
        private System.Threading.ManualResetEvent doneEvent;
        
        public CountingLock(int counts) {
            countsLeft = counts;
            // Mark the event as unsignaled.
            doneEvent = new System.Threading.ManualResetEvent(false);
        }

        public void Decrement() {
            System.Threading.Interlocked.Decrement(ref countsLeft);
            if (countsLeft == 0)
                doneEvent.Set();
        }
        
        public void WaitFor() {
            // The call to FinishedEvent.WaitOne sets the event to wait until
            // FinishedEvent.Set() occurs when the final batch completes
            doneEvent.WaitOne(System.Threading.Timeout.Infinite, true);
        }
    }
    
    private class ForIntBatch {
        int begin; // inclusive
        int end; // exclusive
        System.Action<int> body;

        CountingLock cLock;
        
        public ForIntBatch(int begin, int end, System.Action<int> body, CountingLock cLock) {
            this.begin = begin; this.end = end; 
            this.body = body; this.cLock = cLock;
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(Execute));
        }

        public void Execute(object ignored) {
            for (int i = begin; i < end; ++i)
                body(i);

            cLock.Decrement();
        }
    }

    public static void For(int count, System.Action<int> body, uint batchCountHint = 0) {
        For(0, count, body, batchCountHint);
    }
        
    public static void For(int begin, int count, System.Action<int> body, uint batchCountHint = 0) {
        if (!Supported()) {
            for (int i = begin; i < begin + count; ++i)
                body(i);
        } else {

            uint _elementsPrBatch, _batchCount;
            ComputeBatchInfo((uint)count, batchCountHint, out _elementsPrBatch, out _batchCount);
            int elementsPrBatch = (int)_elementsPrBatch;
            int batchCount = (int)_batchCount;

            CountingLock cLock = new CountingLock((int)batchCount);

            int i;
            for (i = begin; i < begin + count - elementsPrBatch; i += elementsPrBatch)
                new ForIntBatch(i, i + elementsPrBatch, body, cLock);

            // Queue the rest
            new ForIntBatch(i, begin + count, body, cLock);
            
            // Block main thread while batches are executing.
            cLock.WaitFor();
        }
    }

    private class ForUintBatch {
        uint begin; // inclusive
        uint end; // exclusive
        System.Action<uint> body;

        CountingLock cLock;
        
        public ForUintBatch(uint begin, uint end, System.Action<uint> body, CountingLock cLock) {
            this.begin = begin; this.end = end; 
            this.body = body; this.cLock = cLock;
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(Execute));
        }

        public void Execute(object ignored) {
            for (uint i = begin; i < end; ++i)
                body(i);

            cLock.Decrement();
        }
    }

    public static void For(uint count, System.Action<uint> body) {
        For(0, count, body);
    }

    public static void For(uint begin, uint count, System.Action<uint> body, uint batchCountHint = 0) {
        if (!Supported()) {
            for (uint i = begin; i < begin + count; ++i)
                body(i);
        } else {
            uint elementsPrBatch, batchCount;
            ComputeBatchInfo(count, batchCountHint, out elementsPrBatch, out batchCount);

            CountingLock cLock = new CountingLock((int)batchCount);

            uint i;
            for (i = begin; i < begin + count - elementsPrBatch; i += elementsPrBatch)
                new ForUintBatch(i, i + elementsPrBatch, body, cLock);

            // Queue the rest
            new ForUintBatch(i, begin + count, body, cLock);
            
            // Block main thread while batches are executing.
            cLock.WaitFor();
        }
    }

    //! TODO Use IEnumerable<T> instead to only implement one ForEach, but Unity/mono only has a limited implementation of it compared to C#!!!
    public static void ForEach<T>(System.Collections.Generic.List<T> source, System.Action<T> body, uint batchCountHint = 0) {
        For(0, source.Count, 
            i => { body(source[i]); },
            batchCountHint);
    }

    public static void ForEach<T>(T[] source, System.Action<T> body, uint batchCountHint = 0) {
        For(0, source.Length, 
            i => { body(source[i]); },
            batchCountHint);
    }

    // TODO ForEach over enums

    private static void ComputeBatchInfo(uint elementCount, uint batchCountHint, out uint elementsPrBatch, out uint batchCount) {
        if (batchCountHint == 0) {
            // batchCountHint = System.Environment.ProcessorCount;
            
            // Number of batches hint should the the log2 of the count
            batchCountHint = (uint)Mathf.CeilToInt(Mathf.Log((float)elementCount, 2.0f));
            
            // ... but make sure it's a multiple of the processor count (You don't want it being processor count + 1 for instance)
            uint processorCount = (uint)System.Environment.ProcessorCount;
            batchCountHint = CeilDivide(batchCountHint, processorCount) * processorCount;
        }

        if (batchCountHint == 0)
            batchCountHint = (uint)System.Environment.ProcessorCount;

        elementsPrBatch = CeilDivide(elementCount, batchCountHint);
        batchCount = CeilDivide(elementCount, elementsPrBatch);
    }
    
    private static uint CeilDivide(uint lhs, uint rhs) {
        return 1 + ((lhs - 1) / rhs);
    }
}

} // NS Threading