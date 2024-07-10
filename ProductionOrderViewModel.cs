using CommunityToolkit.Mvvm.ComponentModel;

namespace OrdersManagement.View_models
{
    public class ProductionOrderViewModel : ObservableObject
    {
        private readonly ProductionOrder productionOrder;

        public int id => productionOrder.id;
        public string OrderCode => productionOrder.orderCode;
        public string ProductCode => productionOrder.rotorCode;
        public string Description => productionOrder.description;
        public int Quantity => productionOrder.quantityLeft;

        public int rejectedPiecesProcessed => productionOrder.rejectedPiecesProcessed;
        public int goodPiecesProcessed => productionOrder.goodPiecesProcessed;
        public OrderState State => productionOrder.state;

        public double weight => productionOrder.weight; 
        public double correctionRadius => productionOrder.correctionRadius; 

        //public string CampoPerCliente1 => productionOrder.campoPerCliente1;

        public ProductionOrderViewModel(ProductionOrder productionOrder)
        {
            this.productionOrder = productionOrder;
        }
    }
}
