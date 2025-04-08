using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rul.Entities;

namespace Rul.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        List<Product> productList = new List<Product>();

        public string Total
        {
            get
            {
                var total = productList.Sum(p => Convert.ToDouble(p.ProductCost) - Convert.ToDouble(p.ProductCost) * Convert.ToDouble(p.ProductDiscountAmount / 100.00));
                return total.ToString();
            }
        }

        public OrderPage(List<Product> products, User user)
        {
            InitializeComponent();

            DataContext = this;
            productList = products;
            LViewOrder.ItemsSource = productList;

            cmbPickupPoint.ItemsSource = TradeEntities.GetContext().PickupPoint.ToList();

            if (user != null) 
            {
                txtUser.Text = $"{user.UserSurname.ToString()} {user.UserName.ToString()} {user.UserPatronymic.ToString()}";
            }
        }

        private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить этот элемент?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                productList.Remove(LViewOrder.SelectedItem as Product);
            LViewOrder.ItemsSource = null;
            LViewOrder.ItemsSource = productList;
        }

        private void btnOrderSave_Click(object sender, RoutedEventArgs e)
        {
            var productArticle = productList.Select(p => p.ProductArticleNumber).ToArray();
            Random random = new Random();
            var date = DateTime.Now;
            if (productList.Any(p => p.ProductQuantityInStock < 3))
            {
                date = date.AddDays(6);
            } 
            else
            {
                date = date.AddDays(3);
            }

            if (cmbPickupPoint.SelectedItem == null)
            {
                MessageBox.Show("Выберите пункт выдачи!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Order newOrder = new Order()
                {
                    OrderStatus = "Новый",
                    OrderData = DateTime.Now,
                    OrderPickupPoint = cmbPickupPoint.SelectedIndex + 1,
                    OrderDeliveryDate = date,
                    ReceiptCode = random.Next(100, 1000),
                    ClientFullName = txtUser.Text
                };
                TradeEntities.GetContext().Order.Add(newOrder);

                var productCount = new Dictionary<string, int>();

                for (int i = 0; i < productArticle.Count(); i++)
                {
                    var articleNumber = productArticle[i];
                    if (productCount.ContainsKey(articleNumber))
                    {
                        productCount[articleNumber]++;
                    }
                    else
                    {
                        productCount[articleNumber] = 1;
                    }
                }

                foreach (var entry in productCount)
                {
                    OrderProduct newOrderProduct = new OrderProduct()
                    {
                        OrderID = newOrder.OrderID,
                        ProductArticleNumber = entry.Key,
                        Quantity = entry.Value
                    };
                    TradeEntities.GetContext().OrderProduct.Add(newOrderProduct);
                }

                TradeEntities.GetContext().SaveChanges();
                MessageBox.Show("Заказ оформлен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new OrderTicketPage(newOrder, productList));
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
               TradeEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                LViewOrder.ItemsSource =productList;
            }
        }
    }
}
