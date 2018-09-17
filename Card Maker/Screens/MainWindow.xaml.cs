using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Card_Maker.Enums;
using Card_Maker.Models;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Card_Maker.Screens;
using System.Windows.Controls;

namespace Card_Maker
{
    public partial class MainWindow : Window
    {
        private List<EffectItem> effects = new List<EffectItem>();

        private const int cardWidth = 817;
        private const int cardHeight = 1108;
        private Rectangle cropRect;

        private const int inactiveTime = 1;
        private int currentInactiveTime = 1;

        private System.Windows.Point oldPos;
        private System.Windows.Point newPos;

        private DispatcherTimer timer = new DispatcherTimer();

        private Uri tempUri = new Uri(Path.GetTempPath() + "LastDraw.png");

        public Card currentCard = new Card();

        private float Scale = 1;

        public MainWindow()
        {
            cropRect = new Rectangle(0, 0, cardWidth, cardHeight);

            InitializeComponent();

            SetUpRoles();
            SetUpRollTypes();
            SetupPhases();
            SetUpCritIcons();

            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (currentInactiveTime <= 0)
            {
                timer.Stop();

                DrawCard(currentCard);
            }

            currentInactiveTime -= 1;
        }

        private void SetUpCritIcons()
        {
            foreach (string icons in Enum.GetNames(typeof(CritBar)))
            {
                cbbCritIcons.Items.Add(icons);
            }
        }

        private void SetUpRoles()
        {
            foreach (var role in Enum.GetNames(typeof(Role)))
            {
                cbbMainRole.Items.Add(role);
                lbxSecondaryRoles.Items.Add(role);
            }
        }

        private void SetUpRollTypes()
        {
            foreach (string rollType in Enum.GetNames(typeof(RollType)))
            {
                cbbRollType.Items.Add(rollType);
            }
        }

        private void SetupPhases()
        {
            foreach (string phase in Enum.GetNames(typeof(Phase)))
            {
                cbbPhase.Items.Add(phase);
            }
        }

        private void slCost_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lbCost.Content = "Card Cost: " + slCost.Value;
            currentCard.Cost = System.Convert.ToInt16(slCost.Value);
            DrawCard(currentCard);
        }

        private void btnBrowseBackground_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Cards\\Backgrounds\\"))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Cards\\Backgrounds\\");
                }

                File.Copy(dialog.FileName,
                    AppDomain.CurrentDomain.BaseDirectory + "Cards\\Backgrounds\\" + dialog.SafeFileName, true);

                tbPath.Text = "Cards\\Backgrounds\\" + dialog.SafeFileName;
                Scale = 1;
                SlScale.Value = 1;
                DrawCard(currentCard);
            }
        }

        private void btnSaveCard_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Cards\\"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Cards\\");
            }

            dialog.InitialDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "Cards\\");

            dialog.FileName = tbCardName.Text;
            dialog.DefaultExt = ".png";
            dialog.Filter = "(.png)|*.png";

            if (dialog.ShowDialog() == true)
            {
                string filename = dialog.FileName;
                currentCard.Filename = dialog.SafeFileName;

                if (File.Exists(dialog.FileName)) File.Delete(dialog.FileName);
                File.Copy(tempUri.AbsolutePath, dialog.FileName);

                lbLatestEvent.Content = "Card saved to: " + filename;

                currentCard.SaveToJson();
            }
        }

        private void DrawCard(Card cardInfo)
        {
            using (Bitmap card = new Bitmap(cardWidth, cardHeight))
            {
                Bitmap background = null;
                try
                {

                    Bitmap bitmap = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + currentCard.BackgroundPath);
                    background =
                        Crop(new Bitmap(bitmap, (int)Math.Round(bitmap.Width * Scale), (int)Math.Round(bitmap.Height * Scale)),
                            cropRect); // your source images - assuming they're the same size
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    background = Properties.Resources.CardBack;
                }

                var graphics = Graphics.FromImage(card);
                graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

                // Draw base
                graphics.DrawImage(card, 0, 0, cardWidth, cardHeight);
                graphics.DrawImage(background, cardInfo.BackgroundOffset.X, cardInfo.BackgroundOffset.Y, cardWidth,
                    cardHeight);
                graphics.DrawImage(Properties.Resources.CardFrame, 0, 0, cardWidth, cardHeight);

                // Draw Critbar
                if (!string.IsNullOrWhiteSpace(tbFirstBar.Text) ||
                    !string.IsNullOrWhiteSpace(tbSecondBar.Text))
                {
                    switch (cbbCritIcons.SelectedIndex)
                    {
                        case (int)CritBar.D6:
                            graphics.DrawImage(Properties.Resources.CritD6Bar, 0, 0, cardWidth, cardHeight);
                            break;
                        case (int)CritBar.Positive:
                            graphics.DrawImage(Properties.Resources.CritGoodBar, 0, 0, cardWidth, cardHeight);
                            break;
                        case (int)CritBar.Negative:
                            graphics.DrawImage(Properties.Resources.CritBadBar, 0, 0, cardWidth, cardHeight);
                            break;
                        case (int)CritBar.Both:
                            graphics.DrawImage(Properties.Resources.CritBothBar, 0, 0, cardWidth, cardHeight);
                            break;
                    }
                }

                // Draw Rollbar
                graphics.DrawImage(Properties.Resources.RollBar, 0, 0, cardWidth, cardHeight);
                switch (cbbRollType.SelectedIndex)
                {
                    case (int)RollType.Skillcheck:
                        graphics.DrawImage(Properties.Resources.d20Icon, 0, 0, cardWidth, cardHeight);
                        break;
                    case (int)RollType.PushRoll:
                        graphics.DrawImage(Properties.Resources.d6Icon, 0, 0, cardWidth, cardHeight);
                        break;
                    case (int)RollType.Boost:
                        //TODO: Missing Boost icon
                        break;
                }

                // Draw Title
                if (cardInfo.Cost > 0)
                {
                    graphics.DrawImage(Properties.Resources.TitleBar, 0, 0, cardWidth, cardHeight);
                }
                else
                {
                    graphics.DrawImage(Properties.Resources.TitleBarSlim, 0, 0, cardWidth, cardHeight);
                }

                // Draw Roles
                try
                {
                    DrawRoles(graphics);
                }
                catch { }

                if (!string.IsNullOrWhiteSpace(cardInfo.Summary))
                {
                    using (Font NerisBig = new Font("Conthrax Sb", 14, System.Drawing.FontStyle.Bold))
                    {
                        SizeF PhaseSize = graphics.MeasureString(SplitCamelCaseExtension.SplitCamelCase(cardInfo.Summary), NerisBig, 817);
                        graphics.DrawString(SplitCamelCaseExtension.SplitCamelCase(cardInfo.Summary), NerisBig, System.Drawing.Brushes.Gray,
                            new PointF(cardWidth / 2 - PhaseSize.Width / 2, 62));
                    }
                }

                using (Font MainHeading = new Font("Conthrax Sb", 27))
                {
                    SizeF titleSize = graphics.MeasureString(cardInfo.Name, MainHeading, 500);
                    graphics.DrawString(cardInfo.Name, MainHeading, System.Drawing.Brushes.Black,
                        new PointF(cardWidth / 2 - titleSize.Width / 2, 105));

                    SizeF rollTypeSize = graphics.MeasureString(cardInfo.RollText, MainHeading, 500);
                    graphics.DrawString(cardInfo.RollText, MainHeading, System.Drawing.Brushes.White,
                        new PointF(cardWidth / 2 - rollTypeSize.Width / 2, 525));


                    if (cardInfo.Cost > 0)
                    {
                        SizeF costSize = graphics.MeasureString(cardInfo.Cost.ToString(), MainHeading, 500);
                        graphics.DrawString(cardInfo.Cost.ToString(), MainHeading, System.Drawing.Brushes.White,
                            new PointF(709 - costSize.Width / 2, 130 - costSize.Height / 2));
                    }
                }

                using (Font NerisBig = new Font("Conthrax Sb", 12, System.Drawing.FontStyle.Regular))
                {
                    SizeF PhaseSize = graphics.MeasureString(
    SplitCamelCaseExtension.SplitCamelCase(Enum.GetName(typeof(Phase), cardInfo.Phase)), NerisBig, 500);
                    graphics.DrawString(
                        SplitCamelCaseExtension.SplitCamelCase(Enum.GetName(typeof(Phase), cardInfo.Phase)), NerisBig,
                        System.Drawing.Brushes.Orange, new PointF(cardWidth / 2 - PhaseSize.Width / 2, 160));
                }

                using (Font NerisBig = new Font("Conthrax Sb", 20, System.Drawing.FontStyle.Regular))
                {
                    SizeF rollTargetSize = graphics.MeasureString(tbRollTarget.Text, NerisBig, 500);
                    graphics.DrawString(cardInfo.RollTarget, NerisBig, System.Drawing.Brushes.Orange,
                        new PointF(cardWidth / 2 - rollTargetSize.Width / 2, 585));
                }

                using (Font Body = new Font("Neris Light", 17, System.Drawing.FontStyle.Regular))
                {
                    RectangleF DescriptionRect = new RectangleF(75, 620, 680, 300);
                    graphics.DrawString(cardInfo.Description, Body, System.Drawing.Brushes.White, DescriptionRect);
                }

                using (Font Critical = new Font("Neris Black", 24))
                {
                    switch (currentCard.CritIcon)
                    {
                        case CritBar.Positive:
                            graphics.DrawString(cardInfo.CriticalSuccess, Critical, System.Drawing.Brushes.White,
                                new PointF(125, 1000));
                            break;
                        case CritBar.Negative:
                            graphics.DrawString(cardInfo.CriticalFailure, Critical, System.Drawing.Brushes.White,
                                new PointF(125, 1000));
                            break;
                        case CritBar.Both:
                            graphics.DrawString(cardInfo.CriticalSuccess, Critical, System.Drawing.Brushes.White,
                                new PointF(125, 932));

                            graphics.DrawString(cardInfo.CriticalFailure, Critical, System.Drawing.Brushes.White,
                                new PointF(125, 1000));
                            break;
                        case CritBar.D6:
                            graphics.DrawString(cardInfo.CriticalSuccess, Critical, System.Drawing.Brushes.White,
                                new PointF(125, 1000));
                            break;
                    }
                }

                SaveFile(card);

                SetupPreview();
                if (lbLatestEvent != null)
                    lbLatestEvent.Content = "Card preview updated";
            }
        }

        private static void SaveFile(Bitmap card)
        {
            using (Bitmap image = new Bitmap(card))
            {
                string outputFileName = Path.GetTempPath() + "LastDraw.png";
                using (MemoryStream memory = new MemoryStream())
                {
                    using (FileStream fs = new FileStream(outputFileName, FileMode.Create, FileAccess.ReadWrite,
                        FileShare.ReadWrite))
                    {
                        image.Save(memory, ImageFormat.Png);
                        byte[] bytes = memory.ToArray();
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
            }
        }

        private void SetupPreview()
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = tempUri;
            image.EndInit();
            canvas.Source = image;
        }

        private void DrawRoles(Graphics g)
        {
            List<Bitmap> roles = GetRoleBitmaps();

            // Draw main role, it always exists
            g.DrawImage(roles[0], 73, 87, 69, 78);

            switch (roles.Count)
            {
                case 2:
                    // Frames
                    g.DrawImage(Properties.Resources.SecondaryHex, 0, 0, cardWidth, cardHeight);
                    // Icons
                    g.DrawImage(roles[1], 111, 156, 69, 78);
                    break;
                case 3:
                    // Frames
                    g.DrawImage(Properties.Resources.SecondaryHex, 0, 0, cardWidth, cardHeight);
                    g.DrawImage(Properties.Resources.SecondaryHex2, 0, 0, cardWidth, cardHeight);
                    // Icons
                    g.DrawImage(roles[1], 111, 156, 69, 78);
                    g.DrawImage(roles[2], 73, 221, 69, 78);
                    break;
                case 4:
                    // Frames
                    g.DrawImage(Properties.Resources.SecondaryHex, 0, 0, cardWidth, cardHeight);
                    g.DrawImage(Properties.Resources.SecondaryHex2, 0, 0, cardWidth, cardHeight);
                    g.DrawImage(Properties.Resources.SecondaryHex3, 0, 0, cardWidth, cardHeight);
                    // Icons
                    g.DrawImage(roles[1], 111, 156, 69, 78);
                    g.DrawImage(roles[2], 73, 221, 69, 78);
                    g.DrawImage(roles[3], 109, 286, 69, 78);
                    break;
                case 5:
                    // Frames
                    g.DrawImage(Properties.Resources.SecondaryHex, 0, 0, cardWidth, cardHeight);
                    g.DrawImage(Properties.Resources.SecondaryHex2, 0, 0, cardWidth, cardHeight);
                    g.DrawImage(Properties.Resources.SecondaryHex3, 0, 0, cardWidth, cardHeight);
                    g.DrawImage(Properties.Resources.SecondaryHex4, 0, 0, cardWidth, cardHeight);
                    // Icons
                    g.DrawImage(roles[1], 111, 156, 69, 78);
                    g.DrawImage(roles[2], 73, 221, 69, 78);
                    g.DrawImage(roles[3], 109, 286, 69, 78);
                    g.DrawImage(roles[4], 72, 349, 69, 78);
                    break;
                case 6:
                    // Frames
                    g.DrawImage(Properties.Resources.SecondaryHex, 0, 0, cardWidth, cardHeight);
                    g.DrawImage(Properties.Resources.SecondaryHex2, 0, 0, cardWidth, cardHeight);
                    g.DrawImage(Properties.Resources.SecondaryHex3, 0, 0, cardWidth, cardHeight);
                    g.DrawImage(Properties.Resources.SecondaryHex4, 0, 0, cardWidth, cardHeight);
                    g.DrawImage(Properties.Resources.SecondaryHex5, 0, 0, cardWidth, cardHeight);
                    // Icons
                    g.DrawImage(roles[1], 111, 156, 69, 78);
                    g.DrawImage(roles[2], 73, 221, 69, 78);
                    g.DrawImage(roles[3], 109, 286, 69, 78);
                    g.DrawImage(roles[4], 72, 349, 69, 78);
                    g.DrawImage(roles[5], 110, 413, 69, 78);
                    break;
            }
        }

        private Bitmap Crop(Bitmap src, Rectangle rect)
        {
            Bitmap result = new Bitmap(rect.Width, rect.Height);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(src, new Rectangle(0, 0, result.Width, result.Height),
                    rect,
                    GraphicsUnit.Pixel);
            }

            return result;
        }

        private List<Role> GetRoles()
        {
            List<Role> roles = new List<Role> { Role.None };

            if (cbbMainRole.SelectedItem != null) roles[0] = (Role)Enum.Parse(typeof(Role), cbbMainRole.SelectedItem.ToString());

            foreach (string item in lbxSecondaryRoles.SelectedItems)
            {
                roles.Add((Role)Enum.Parse(typeof(Role), item));
            }

            return roles;
        }

        private List<Bitmap> GetRoleBitmaps()
        {
            List<Bitmap> roles = new List<Bitmap>();

            foreach (Role role in currentCard.Roles)
            {
                roles.Add(GetRoleBitmap(role));
            }

            return roles;
        }

        private Bitmap GetRoleBitmap(Role role)
        {
            switch (role)
            {
                case Role.Pilot:
                    return Properties.Resources.Pilot;
                case Role.Gunner:
                    return Properties.Resources.Gunner;
                case Role.Captain:
                    return Properties.Resources.Captain;
                case Role.Mage:
                    return Properties.Resources.Mage;
                case Role.ScienceOfficer:
                    return Properties.Resources.Science;
                case Role.Engineering:
                    return Properties.Resources.Engineer;
                default:
                    return new Bitmap(0, 0);
            }
        }

        private void tbDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentCard.Description = tbDescription.Text;
            StartTimer();
        }


        private void tbSummary_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentCard.Summary = tbSummary.Text;
            StartTimer();
        }

        private void StartTimer()
        {
            currentInactiveTime = inactiveTime;
            timer.Start();
        }

        private void tbCardName_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentCard.Name = tbCardName.Text;
            StartTimer();
        }

        private void tbRollTarget_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentCard.RollTarget = tbRollTarget.Text;
            StartTimer();
        }

        private void tbRollType_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentCard.RollText = tbRollType.Text;
            StartTimer();
        }

        private void cbbRollType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentCard.RollType = (RollType)Enum.Parse(typeof(RollType), cbbRollType.SelectedItem.ToString());
            DrawCard(currentCard);
        }

        private void cbbMainRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentCard.Roles.Count > 0)
            {
                currentCard.Roles[0] = (Role)Enum.Parse(typeof(Role), cbbMainRole.SelectedItem.ToString());
            }
            else
            {
                currentCard.Roles.Add((Role)Enum.Parse(typeof(Role), cbbMainRole.SelectedItem.ToString()));
            }

            DrawCard(currentCard);
        }

        private void cbbCritIcons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentCard.CritIcon = (CritBar)Enum.Parse(typeof(CritBar), cbbCritIcons.SelectedItem.ToString());

            switch (currentCard.CritIcon)
            {
                case CritBar.Positive:
                    lbFirstBar.IsEnabled = true;
                    tbFirstBar.IsEnabled = true;
                    tbSecondBar.IsEnabled = false;
                    tbSecondBar.IsEnabled = false;
                    break;
                case CritBar.Negative:
                    lbFirstBar.IsEnabled = false;
                    tbFirstBar.IsEnabled = false;
                    tbSecondBar.IsEnabled = true;
                    tbSecondBar.IsEnabled = true;
                    break;
                case CritBar.Both:
                    lbFirstBar.IsEnabled = true;
                    tbFirstBar.IsEnabled = true;
                    tbSecondBar.IsEnabled = true;
                    tbSecondBar.IsEnabled = true;
                    break;
                case CritBar.D6:
                    lbFirstBar.IsEnabled = true;
                    tbFirstBar.IsEnabled = true;
                    tbSecondBar.IsEnabled = false;
                    tbSecondBar.IsEnabled = false;
                    break;
            }

            DrawCard(currentCard);
        }

        private void tbFirstBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentCard.CriticalSuccess = tbFirstBar.Text;
            StartTimer();
        }

        private void tbSecondBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentCard.CriticalFailure = tbSecondBar.Text;
            StartTimer();
        }

        private void tbPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentCard.BackgroundPath = tbPath.Text;
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            newPos = e.GetPosition(canvas);

            PointF offset = currentCard.BackgroundOffset;

            if (newPos.X > oldPos.X)
            {
                offset.X += (float)(newPos.X - oldPos.X);
            }
            else
            {
                offset.X -= (float)(oldPos.X - newPos.X);
            }

            if (newPos.Y > oldPos.Y)
            {
                offset.Y += (float)(newPos.Y - oldPos.Y);
            }
            else
            {
                offset.Y -= (float)(oldPos.Y - newPos.Y);
            }

            currentCard.BackgroundOffset = offset;

            lbLatestEvent.Content = currentCard.BackgroundOffset.X + "," + currentCard.BackgroundOffset.Y;

            DrawCard(currentCard);
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            oldPos = e.GetPosition(canvas);
        }

        private void btnCards_Click(object sender, RoutedEventArgs e)
        {
            Cards cards = new Cards();
            if (cards.ShowDialog() == true)
            {
                lbLatestEvent.Content = "Card selected: " + cards.SelectedCard.Name;
                currentCard = cards.SelectedCard;

                // Update UI
                newPos = new System.Windows.Point(0, 0);
                oldPos = new System.Windows.Point(0, 0);
                UpdateFields(currentCard);
                DrawCard(currentCard);
            }
        }

        private void UpdateFields(Card card)
        {
            tbCardName.Text = card.Name;
            tbFirstBar.Text = card.CriticalSuccess;
            tbSecondBar.Text = card.CriticalFailure;
            tbRollTarget.Text = card.RollTarget;
            tbRollType.Text = card.RollText;
            tbPath.Text = card.BackgroundPath;
            tbDescription.Text = card.Description;
            tbSummary.Text = card.Summary;
            SlScale.Value = card.Scale;

            cbbCritIcons.SelectedIndex = (int)card.CritIcon;
            if (card.Roles.Count > 0) cbbMainRole.SelectedIndex = (int)card.Roles[0];
            cbbPhase.SelectedIndex = (int)card.Phase;
            cbbRollType.SelectedIndex = (int)card.RollType;

            lbxSecondaryRoles.SelectedItems.Clear();

            int i = 1;
            while (i < card.Roles.Count)
            {
                // Get item's ListBoxItem
                lbxSecondaryRoles.SelectedItems.Add(lbxSecondaryRoles.Items[(int)card.Roles[i]]);

                i++;
            }

            slCost.Value = card.Cost;
        }

        private void cbbPhase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentCard.Phase = (Phase)Enum.Parse(typeof(Phase), cbbPhase.SelectedItem.ToString());
            DrawCard(currentCard);
        }

        private void lbxSecondaryRoles_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentCard.Roles = GetRoles();
            DrawCard(currentCard);
        }

        private void SlScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Scale = (float)((Slider)sender).Value;
            currentCard.Scale = Scale;
            DrawCard(currentCard);
        }
    }
}