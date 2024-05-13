using MySql.Data.MySqlClient;
using System.Data;

namespace Ledgers_Server_Main.Classes
{
    public class MySQL
    {
        private readonly MySqlConnection _connection;

        public MySQL(string host, string user, string password, string database)
        {
            _connection = new MySqlConnection($"server={host};user={user};password={password};database={database};");
        }

        public DataTable Read(string table, Dictionary<string, string[]>? where = null)
        {
            DataTable dataTable = new DataTable();
            var query = $"SELECT * FROM {table}";
            if (where is not null)
            {
                var conditions = where.Select(pair =>
                    $"{pair.Key} IN ({string.Join(", ", pair.Value.Select((value, index) => $"@{pair.Key}{index}"))})");
                query = $"{query} WHERE {string.Join(" AND ", conditions)}";
            }
            try
            {
                _connection.Open();
                using (var command = new MySqlCommand($"{query};", _connection))
                {
                    if (where is not null)
                    {
                        foreach (var pair in where)
                        {
                            for (int i = 0; i < pair.Value.Length; i++)
                            {
                                command.Parameters.AddWithValue($"@{pair.Key}{i}", pair.Value[i]);
                            }
                        }
                    }
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Traceback: " + ex.StackTrace);
                throw;
            }
            finally
            {
                _connection.Close();
            }
            return dataTable;
        }

        public DataTable Read(string query, string[] parameters)
        {
            DataTable dataTable = new DataTable();
            try
            {
                _connection.Open();
                using (var command = new MySqlCommand($"{query};", _connection))
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Traceback: " + ex.StackTrace);
                throw;
            }
            finally
            {
                _connection.Close();
            }
            return dataTable;
        }

        public void Insert(string table, Dictionary<string, dynamic>[] items)
        {
            if (items.Length == 0) return;

            var columns = string.Join(", ", items[0].Keys.Select(k => $"`{k}`"));
            var values = string.Join(", ", items[0].Keys.Select(k => $"@{k}"));
            var query = $"INSERT INTO `{table}` ({columns}) VALUES ({values})";

            try
            {
                _connection.Open();
                using (var command = new MySqlCommand(query, _connection))
                {
                    foreach (var item in items)
                    {
                        command.Parameters.Clear();
                        foreach (var kvp in item)
                        {
                            command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                        }
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Traceback: " + ex.StackTrace);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Update(string table, Dictionary<string, dynamic>[] items, string key = "id")
        {
            if (items.Length == 0) throw new ArgumentException();

            var properties = items[0].Keys.ToArray();
            var clause = string.Join(", ", properties.Where(p => p != key).Select(p => $"`{p}` = @{p}"));
            var query = $"UPDATE `{table}` SET {clause} WHERE `{key}` = @{key}";

            try
            {
                _connection.Open();
                using (var command = new MySqlCommand(query, _connection))
                {
                    foreach (var item in items)
                    {
                        command.Parameters.Clear();
                        foreach (var prop in properties)
                        {
                            command.Parameters.AddWithValue($"@{prop}", item[prop] ?? DBNull.Value);
                        }
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Traceback: " + ex.StackTrace);
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        public int Delete(string table, string id, string column = "id")
        {
            var affectedRows = 0;
            var query = $"DELETE FROM {table} WHERE {column} = @id";
            try
            {
                _connection.Open();
                using (var command = new MySqlCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    affectedRows = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Traceback: " + ex.StackTrace);
                throw;
            }
            finally
            {
                _connection.Close();
            }
            return affectedRows;
        }
    }
}
