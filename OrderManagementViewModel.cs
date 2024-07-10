using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using EBSOrdersManager;
using System;
using System.IO;

namespace OrdersManagement.View_models
{
    public partial class OrderManagementViewModel : ObservableObject
    {
        [ObservableProperty] public ObservableCollection<ProductionOrderViewModel> orders;
        static public ProductionOrder selectedOrder { get; private set; }

        private OrdersManagement_ViaDB ordersManagement;

        [ObservableProperty] public int selectedOrderIndex;
        [ObservableProperty] public string inputOrderCode;
        [ObservableProperty] public string inputProductCode;
        [ObservableProperty] public string inputNewValue;
        [ObservableProperty] public string orderCodeHeader;
        [ObservableProperty] public string productCodeHeader;
        [ObservableProperty] public string descriptionHeader;
        [ObservableProperty] public string quantityHeader;
        [ObservableProperty] public string title;

        [ObservableProperty] private string searchedText;

        private const string DATABASE_IP_PATH = @"./database_ip.txt";

        public ProductionOrderViewModel SelectedOrder
        {
            get
            {
                return new ProductionOrderViewModel(selectedOrder);
            }
            set
            {
                if (value != null)
                    selectedOrder = new ProductionOrder(value.id, value.OrderCode, value.ProductCode, value.Quantity, value.Description, value.State, value.rejectedPiecesProcessed, value.goodPiecesProcessed, value.weight, value.correctionRadius);
                else
                    selectedOrder = null;

                OnPropertyChanged();
            }
        }

        public OrderManagementViewModel()
        {
            ordersManagement = OrdersManagement_ViaDB.Instance;

            Orders = new ObservableCollection<ProductionOrderViewModel>();
            //ordersManagement.Attach(this);

            OrderCodeHeader = "Codice ordine";//TApplicationMessages.GetLabelById("order_code_header");
            ProductCodeHeader = "Codice rotore";//TApplicationMessages.GetLabelById("product_code_header");
            DescriptionHeader = "Descrizione";//TApplicationMessages.GetLabelById("order_description_header");
            QuantityHeader = "Quantità";//TApplicationMessages.GetLabelById("quantity_header");
            Title = "Ordini";//TApplicationMessages.GetLabelById("orders_title");

            try
            {
                StringInputDialogBox dialog;
                bool isIpValid;
                string ip;
                if (File.Exists(DATABASE_IP_PATH))
                {
                    ip = File.ReadAllText(DATABASE_IP_PATH);
                }
                else
                {
                    dialog = new StringInputDialogBox("Inserisci l'ip del database", false);
                    dialog.ShowDialog();
                    ip = dialog.output;
                }

                isIpValid = ordersManagement.TryToConnectToDb(ip);

                while (!isIpValid)
                {
                    dialog = new StringInputDialogBox("Inserisci l'ip del database", false);
                    dialog.ShowDialog();
                    ip = dialog.output;
                    isIpValid = ordersManagement.TryToConnectToDb(ip);
                }

                File.WriteAllText(DATABASE_IP_PATH, ip);
            }
            catch
            {

            }

            UpdateTable();
        }

        public void UpdateTableSafely(object sender = null, EventArgs e = null)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                UpdateTable();
            });
        }

        public async void UpdateTable()
        {
            Orders.Clear();

            List<ProductionOrder> readOrders = await ordersManagement.GetOrders();

            if (string.IsNullOrEmpty(SearchedText))
            {
                for (short i = 0; i < readOrders.Count; i++)
                {
                    ProductionOrderViewModel order = new(readOrders[i]);
                    Orders.Add(order);
                }
            }
            else
            {
                bool foundElement = false;
                for (int i = 0; i < readOrders.Count; i++)
                {
                    if (readOrders[i].orderCode.ToLower().Contains(SearchedText.ToLower())
                        || readOrders[i].rotorCode.ToLower().Contains(SearchedText.ToLower())
                        || readOrders[i].description.ToLower().Contains(SearchedText.ToLower()))
                    {
                        foundElement = true;
                        ProductionOrderViewModel order = new(readOrders[i]);
                        Orders.Add(order);
                    }
                }

                if (!foundElement)
                {
                    MessageBox.Show("Ordine non trovato");//new DialogBox_Confirm(TApplicationMessages.GetLabelById("order_not_found")).ShowDialog();
                    SearchedText = "";
                    UpdateTableSafely();
                }
            }
        }

        [RelayCommand]
        private void DeleteOrder()
        {
            if (selectedOrder == null)
            {
                MessageBox.Show("Selezionare prima l'ordine da eliminare");
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Confermare l'eliminazione dell'ordine: {selectedOrder.orderCode}?", "Confermare", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                ordersManagement.DeleteOrderById(selectedOrder.id);
                SelectedOrder = null;
                UpdateTableSafely();
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            MessageBoxResult result = MessageBox.Show($"Vuoi chiudere l'applicazione?", "Confermare", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        [RelayCommand]
        private void CreateOrder()
        {
            string orderCode;
            string productCode;
            int quantity;
            string description;

            StringInputDialogBox dialog = new StringInputDialogBox("Inserisci codice ordine");
            bool result = dialog.ShowDialog() ?? false;

            if (result)
                orderCode = dialog.output;
            else
                return;

            dialog = new StringInputDialogBox("Inserisci codice rotore");
            result = dialog.ShowDialog() ?? false;

            if (result)
            {
                if (string.IsNullOrEmpty(dialog.output))
                {
                    MessageBox.Show("Input non valido");
                    return;
                }
               
                productCode = dialog.output;
            }
            else
                return;

            dialog = new StringInputDialogBox("Inserisci descrizione");
            result = dialog.ShowDialog() ?? false;

            if (result)
                description = dialog.output;
            else
                return;

            dialog = new StringInputDialogBox("Inserisci quantità");
            result = dialog.ShowDialog() ?? false;

            if (result)
            {
                if (!int.TryParse(dialog.output, out quantity))
                {
                    MessageBox.Show("Input non numerico");
                    return;
                }
            }
            else
                return;

            //invia id = 0 tanto è un seriale sul database e viene assegnato automaticamente
            ordersManagement.CreateNewOrder(new(0, orderCode, productCode, quantity, description, OrderState.TODO, 0, 0, 0, 0));
            UpdateTableSafely();
        }

        [RelayCommand]
        private void Search()
        {
            StringInputDialogBox dialog = new StringInputDialogBox("Inserisci testo da ricercare");
            bool result = dialog.ShowDialog() ?? false;

            if (result)
            {
                SearchedText = dialog.output;
            }
            else
            {
                SearchedText = "";
            }

            UpdateTableSafely();
        }

        [RelayCommand]
        private void Confirm()
        {

        }

        [RelayCommand]
        private void Cancel()
        {

        }

        public void Update(string aSender)
        {
            UpdateTableSafely();
        }
    }
}
