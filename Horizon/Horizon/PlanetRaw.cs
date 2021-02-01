
namespace Horizon
{
	public class PlanetRaw
	{
        public string status;
        public string message;
        public string calculationId;
        public Column[] columns;

        public string[][] rows;


        public class Column
        {
            string name;
            string type;
            string outputID;
            string units;
        }

        public string printRows()
        {
            string s = "\n";
            for (int i = 0; i < rows.Length; i++)
            {
                for (int j = 0; j < rows[i].Length; j++)
                    s += rows[i][j] + "\n";
                s += "\n";
            }
            return s;
		}


	}
}
