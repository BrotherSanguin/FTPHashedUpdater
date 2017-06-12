/*
 * Created by SharpDevelop.
 * User: Ex
 * Date: 11.10.2015
 * Time: 17:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Windows;

namespace FTPHashedUpdater
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class Main : Window
  {
    public Main()
    {
      InitializeComponent();
      DataContext = new FTPUpdaterModel();
    }
  }
}