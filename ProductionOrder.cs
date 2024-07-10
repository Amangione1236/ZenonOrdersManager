using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrdersManagement
{
    public enum OrderState
    {
        /// <summary>
        /// The quantity is greater than 0 
        /// </summary>
        TODO = 0,

        /// <summary>
        /// The quantity is greater than 0 and the order is selected
        /// </summary>
        ACTIVE = 1,

        /// <summary>
        /// The quantity is 0
        /// </summary>
        COMPLETED = 2
    }

    public class ProductionOrder
    {
        public int id;
        public string orderCode;
        public string rotorCode;
        public string description;
        public int quantityLeft;

        public int goodPiecesProcessed;
        public int rejectedPiecesProcessed;
        public OrderState state;

        public double correctionRadius;
        public double weight;

        public ProductionOrder(int id, string orderCode, string productCode, int quantity, string description, OrderState state, int rejectedPieces, int goodPieces, double weight, double correctionRadius)
        {
            this.id = id;
            this.orderCode = orderCode;
            rotorCode = productCode;
            this.description = description;
            this.quantityLeft = quantity;

            goodPiecesProcessed = goodPieces;
            rejectedPiecesProcessed = rejectedPieces;
            this.state = state;

            this.weight = weight;
            this.correctionRadius = correctionRadius;
        }
    }
}
