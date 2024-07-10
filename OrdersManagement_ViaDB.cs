using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OrdersManagement
{
    public class OrdersManagement_ViaDB
    {
        private const string ORDERS_TABLE_NAME = "orders";
        static public SemaphoreSlim dbSemaphore = new SemaphoreSlim(1, 1);

        private static OrdersManagement_ViaDB instance;
        public static OrdersManagement_ViaDB Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OrdersManagement_ViaDB();
                }

                return instance;
            }
        }

        public enum OrdersColumns
        {
            id,
            orderCode,
            rotorCode,
            quantity,
            description,
            goodPiecesProcessed,
            rejectedPiecesProcessed,
            state,

            weight,
            correction_radius,
        }

        private readonly Dictionary<OrdersColumns, string> ORDERS_COLUMNS = new Dictionary<OrdersColumns, string>()
        {
            { OrdersColumns.id, "id"},
            { OrdersColumns.orderCode, "order_code"},
            { OrdersColumns.rotorCode, "rotor_code"},
            { OrdersColumns.quantity, "quantity"},
            { OrdersColumns.description, "description"},
            { OrdersColumns.goodPiecesProcessed, "good_pieces_processed"},
            { OrdersColumns.rejectedPiecesProcessed, "rejected_pieces_processed"},
            { OrdersColumns.state, "state"},
            { OrdersColumns.weight, "weight"},
            { OrdersColumns.correction_radius, "correction_radius"},
        };

        private NpgsqlConnection connection;

        private OrdersManagement_ViaDB()
        {

        }

        public bool TryToConnectToDb(string ip)
        {
            string CONNECTION_INFO = string.Format(
                "Server={0};Port={1};UserId={2};Password={3};Database={4};",
                ip, 5432, "postgres", "Ebspostgre!", "ZenonData");

            connection = new NpgsqlConnection(CONNECTION_INFO);

            try
            {
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        ~OrdersManagement_ViaDB()
        {
            connection.Close();
        }

        #region CREATE
        public async void CreateNewOrder(ProductionOrder productionOrder)
        {
            string INSERT = $"INSERT INTO {ORDERS_TABLE_NAME} " +
                $"({ORDERS_COLUMNS[OrdersColumns.orderCode]}, {ORDERS_COLUMNS[OrdersColumns.rotorCode]}, {ORDERS_COLUMNS[OrdersColumns.description]}, {ORDERS_COLUMNS[OrdersColumns.quantity]}, {ORDERS_COLUMNS[OrdersColumns.goodPiecesProcessed]}, {ORDERS_COLUMNS[OrdersColumns.rejectedPiecesProcessed]}, {ORDERS_COLUMNS[OrdersColumns.state]}, {ORDERS_COLUMNS[OrdersColumns.weight]}, {ORDERS_COLUMNS[OrdersColumns.correction_radius]})" +
                $" VALUES (@orderCode, @rotorCode, @description, @quantity, @goodPiecesProcessed, @rejectedPiecesProcessed, @state, @weight, @correctionRadius)";

            await dbSemaphore.WaitAsync();
            try
            {
                using (var command = new NpgsqlCommand(INSERT, connection))
                {
                    command.Parameters.AddWithValue("id", productionOrder.id);
                    command.Parameters.AddWithValue("orderCode", productionOrder.orderCode);
                    command.Parameters.AddWithValue("rotorCode", productionOrder.rotorCode);
                    command.Parameters.AddWithValue("description", productionOrder.description);
                    command.Parameters.AddWithValue("quantity", productionOrder.quantityLeft);
                    command.Parameters.AddWithValue("goodPiecesProcessed", productionOrder.goodPiecesProcessed);
                    command.Parameters.AddWithValue("rejectedPiecesProcessed", productionOrder.rejectedPiecesProcessed);
                    command.Parameters.AddWithValue("state", (int)productionOrder.state);
                    command.Parameters.AddWithValue("weight", productionOrder.weight);
                    command.Parameters.AddWithValue("correctionRadius", productionOrder.correctionRadius);

                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                dbSemaphore.Release();
            }
        }
        #endregion

        #region READ
        public async Task<List<ProductionOrder>> GetOrders(bool onlyActive = false)
        {
            List<ProductionOrder> orders = new();

            string SELECT = $"SELECT * FROM {ORDERS_TABLE_NAME}";

            if (onlyActive)
                SELECT += $" WHERE {ORDERS_COLUMNS[OrdersColumns.state]} <> {(int)OrderState.COMPLETED}";

            await dbSemaphore.WaitAsync();
            try
            {
                await using (NpgsqlCommand command = new NpgsqlCommand(SELECT, connection))
                {
                    await using (NpgsqlDataReader reader = command.ExecuteReaderAsync().Result)
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(ReadOrder(reader));
                        }
                    }
                }
            }
            finally
            {
                dbSemaphore.Release();
            }
            return orders;
        }

        private ProductionOrder ReadOrder(NpgsqlDataReader reader)
        {
            int id = reader[ORDERS_COLUMNS[OrdersColumns.id]] as int? ?? 99999;
            string orderCode = reader[ORDERS_COLUMNS[OrdersColumns.orderCode]] as string;
            string rotorCode = reader[ORDERS_COLUMNS[OrdersColumns.rotorCode]] as string;
            string description = reader[ORDERS_COLUMNS[OrdersColumns.description]] as string;
            int quantity = reader[ORDERS_COLUMNS[OrdersColumns.quantity]] as int? ?? 1;
            int goodPieces = reader[ORDERS_COLUMNS[OrdersColumns.goodPiecesProcessed]] as int? ?? 0;
            int rejectedPieces = reader[ORDERS_COLUMNS[OrdersColumns.rejectedPiecesProcessed]] as int? ?? 0;
            int state = reader[ORDERS_COLUMNS[OrdersColumns.state]] as int? ?? 0;

            double weight = reader[ORDERS_COLUMNS[OrdersColumns.weight]] as double? ?? 0;
            double correctionRadius = reader[ORDERS_COLUMNS[OrdersColumns.correction_radius]] as double? ?? 0;

            ProductionOrder order = new(id, orderCode, rotorCode, quantity, description, (OrderState)state, rejectedPieces, goodPieces, weight, correctionRadius);

            return order;
        }
        #endregion

        public async Task UpdateField(int orderId, OrdersColumns column, double newValue)
        {
            string MODIFY_FIELD = $"UPDATE {ORDERS_TABLE_NAME} SET {ORDERS_COLUMNS[column]} = @newValue WHERE {ORDERS_COLUMNS[OrdersColumns.id]} = @id";

            await dbSemaphore.WaitAsync();
            try
            {
                using (var command = new NpgsqlCommand(MODIFY_FIELD, connection))
                {
                    command.Parameters.AddWithValue("newValue", newValue);
                    command.Parameters.AddWithValue("id", orderId);

                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                dbSemaphore.Release();
            }
        }

        #region DELETE
        public async void DeleteOrderById(int ID)
        {
            string DELETE_ORDER_BY_ID = $"DELETE FROM {ORDERS_TABLE_NAME} WHERE {ORDERS_COLUMNS[OrdersColumns.id]} = @ID";

            await dbSemaphore.WaitAsync();
            try
            {
                using (var command = new NpgsqlCommand(DELETE_ORDER_BY_ID, connection))
                {
                    command.Parameters.AddWithValue("ID", ID);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                dbSemaphore.Release();
            }
        }
        #endregion
    }
}
