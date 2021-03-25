namespace Horizon
{
	public class Results
	{
        public string status;
        public string message;
        public string calculationId;
        public Result result;

        public class Result
        {
            public string phase;
        }
        
        public string toString()
        {
            return "\nstatus = " + status + "\nmessage = " + message + "\ncalculationId = " + calculationId + "\nresult.phase = " + result.phase;
        }
    }
}
