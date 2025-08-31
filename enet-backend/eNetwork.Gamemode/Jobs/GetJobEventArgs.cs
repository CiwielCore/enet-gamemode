using System;

namespace eNetwork.Jobs
{
    class GetJobEventArgs : EventArgs
    {
        public int JobId { get; }

        public GetJobEventArgs(int jobId)
        {
            JobId = jobId;
        }
    }
}
