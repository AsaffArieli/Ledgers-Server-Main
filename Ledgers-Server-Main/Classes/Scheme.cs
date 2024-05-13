using System.Data;
using System.Dynamic;
using System.Transactions;

namespace Ledgers_Server_Main.Classes
{
    public class Scheme : Element
    {
        private Dictionary<string, string?> _data = new Dictionary<string, string?>();
        private List<Parameter> _parameters = new List<Parameter>();

        public Scheme(MySQL mySQL, string id) : base(id)
        {
            var dataWhere = new Dictionary<string, string[]> { { "id", new string[] { id } } };
            var data = mySQL.Read(Config.Tables.SCHEMES, dataWhere);
            var parametersWhere = new Dictionary<string, string[]> { { "scheme_id", new string[] { id } } };
            var parameters = mySQL.Read(Config.Tables.PARAMETERS, parametersWhere);
            if (!(data.Rows.Count > 0))
            {
                throw new KeyNotFoundException(id);
            }
            Init(data.Rows[0], parameters);
        }

        public Scheme(DataRow row, DataTable parameters) : base(row)
        {
            parameters = parameters.AsEnumerable().Where(row => row.Field<string>("scheme_id") == Id).CopyToDataTable();
            Init(row, parameters);
        }

        private void Init(DataRow row, DataTable parameters)
        {
            row.Table.Columns.Cast<DataColumn>().ToList()
                .ForEach(column => _data[column.ToString()] = row[column]?.ToString());
            parameters.AsEnumerable().ToList()
                .ForEach(item => _parameters.Add(new Parameter(item)));
        }

        public dynamic GetJSON()
        {
            var expando = new ExpandoObject() as IDictionary<string, object?>;
            _data.ToList().ForEach(pair => expando[pair.Key] = pair.Value);
            expando["parameters"] = _parameters.Select(parameter => parameter.GetJSON());
            return expando;
        }

        public void Delete(MySQL mySQL)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                mySQL.Delete(Config.Tables.SCHEMES, Id);
                _parameters.ForEach(item => item.Delete(mySQL));
                scope.Complete();
            }
        }

        public static List<Scheme> ReadAll(MySQL mySQL)
        {
            var parameters = mySQL.Read(Config.Tables.PARAMETERS);
            return mySQL.Read(Config.Tables.SCHEMES).AsEnumerable()
                .Select<DataRow, Scheme>(scheme => new Scheme(scheme, parameters))
                .ToList();
        }
    }
}
