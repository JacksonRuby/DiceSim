/*
 * FILE         : MainWindow.xaml.cs
 * PROJECT      : Assignment 5 - Dice Roll Simulator
 * PROGRAMMER   : Group 3
 * DATE         : Oct. 8, 2019
 * DESCRIPTION  :
 *      Rolls two dice and saves roll outcomes to a file.
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DiceSim
{
    public partial class MainWindow : Window
    {
        //variable speed for roll
        private int rollSpeed = 0;

        //media player for playing the roll sound
        private readonly MediaPlayer rollSoundPlayer = new MediaPlayer();

        //thread for rolling dice
        private Thread rollThread;

        //all constants
        private const int MIN_ROLL = 0;
        private const int MAX_ROLL = 6;
        private const int SPEED_IMMEDIATE = 0;
        private const int SPEED_FASTER = 10;
        private const int SPEED_FAST = 20;
        private const int SPEED_MODERATE = 45;
        private const int SPEED_SLOW = 70;
        private const int SPEED_SLOWER = 95;
        private const int NUMBER_OF_ROLLS = 20;
        private const string IMMEDIATE = "0";
        private const string FASTER = "1";
        private const string FAST = "2";
        private const string MODERATE = "3";
        private const string SLOW = "4";
        private const string SLOWER = "5";
        private const string SOUND_FILE = "sounds\\tick.mp3";
        private const string OUTPUT_FILE = "DiceOutput.txt";
        private const string IMAGE_FOLDER = "img";
        private const double SPEED_MULTIPLIER = 1.1;


        /*
         * FUNCTION     : MainWindow()
         * DESCRIPTION  :
         *      Occurs on form load. Adds all images to the list.
         */
        public MainWindow()
        {
            InitializeComponent();            

            //set up the roll sound
            rollSoundPlayer.Open(new Uri(SOUND_FILE, UriKind.RelativeOrAbsolute));

            //if the file already exists, write an extra new line to separate previous rolls
            if (File.Exists(OUTPUT_FILE))
                File.AppendAllText(OUTPUT_FILE, Environment.NewLine);
            //string to write to file and display in listview
            string initialRollInfo = "[" + DateTime.Now + "] New set of rolls started.";
            //write initial info to the file
            File.AppendAllText(OUTPUT_FILE, initialRollInfo + Environment.NewLine);
            //display info to the form
            listOutput.Items.Add(initialRollInfo);
        }

        /*
         * FUNCTION     : BtnRoll_Click()
         * DESCRIPTION  :
         *      Occurs on click of the roll button.
         */
        private void BtnRoll_Click(object sender, RoutedEventArgs e)
        {
            //disable the roll button while rolling and reset the progress bar
            btnRoll.IsEnabled = false;
            rollProgressBar.Value = 0;

            //start the thread for the roll          
            rollThread = new Thread(rollTheDice);
            rollThread.Start();
        }

        /*
         * FUNCTION     : rollTheDice()
         * DESCRIPTION  :
         *      Rolls the dice. Runs in a thread.
         */
        private void rollTheDice()
        {
            //get the image files and put them into a list
            List<string> imageLocations = new List<string>();
            imageLocations = Directory.GetFiles(IMAGE_FOLDER).ToList<string>();

            //set up roll random info
            Random rand = new Random();
            int diceRoll1 = 0;
            int diceRoll2 = 0;

            //roll loop for displaying dice images
            int incrementingThreadSleepTime = rollSpeed;
            for (int i = 0; i < NUMBER_OF_ROLLS; i++)
            {
                //get random numbers for dice rolls
                diceRoll1 = rand.Next(MIN_ROLL, MAX_ROLL);
                diceRoll2 = rand.Next(MIN_ROLL, MAX_ROLL);

                //update the form using the dispacher.invoke
                Dispatcher.Invoke(() =>
                {
                    //display the first roll image
                    BitmapImage imageToDisplay = new BitmapImage();
                    imageToDisplay.BeginInit();
                    imageToDisplay.UriSource = new Uri(imageLocations[diceRoll1], UriKind.RelativeOrAbsolute);
                    imageToDisplay.EndInit();
                    imgDice1.Source = imageToDisplay;

                    //display the second roll image
                    BitmapImage imageToDisplay2 = new BitmapImage();
                    imageToDisplay2.BeginInit();
                    imageToDisplay2.UriSource = new Uri(imageLocations[diceRoll2], UriKind.RelativeOrAbsolute);
                    imageToDisplay2.EndInit();
                    imgDice2.Source = imageToDisplay2;

                    //play the roll sound
                    rollSoundPlayer.Play();
                    rollSoundPlayer.Position = new TimeSpan(0);

                    //incrementing progressbar
                    rollProgressBar.Value++;
                });

                //gradually slow the rolling of the dice by sleeping the thread
                incrementingThreadSleepTime = (int)(incrementingThreadSleepTime * SPEED_MULTIPLIER);
                Thread.Sleep(incrementingThreadSleepTime);
            }

            //info for output
            string rollInfo = "[" + DateTime.Now + "] Rolled " + (diceRoll1 + 1) + " and " + (diceRoll2 + 1) + ".";
            //update the listview and reenable the roll button since the roll is now complete
            Dispatcher.Invoke(() =>
            {
                listOutput.Items.Add(rollInfo);
                listOutput.ScrollIntoView(listOutput.Items[listOutput.Items.Count - 1]);
                btnRoll.IsEnabled = true;
            });

            //write roll info to file
            File.AppendAllText(OUTPUT_FILE, rollInfo + Environment.NewLine);
        }

        /*
         * FUNCTION     : MenuCheck()
         * DESCRIPTION  :
         *      Occurs on click of any item in the menu.
         */
        private void MenuCheck(object sender, RoutedEventArgs e)
        {
            //ensure that the only checked menu item is the one the user clicked
            foreach (MenuItem itm in menuRollSpeed.Items)
            {
                itm.IsChecked = (sender as MenuItem)?.Tag == itm.Tag ? true : false;
            }

            //change the roll speed based on the menu item clicked
            switch ((sender as MenuItem)?.Tag)
            {
                case IMMEDIATE:
                    rollSpeed = SPEED_IMMEDIATE;
                    break;
                case FASTER:
                    rollSpeed = SPEED_FASTER;
                    break;
                case FAST:
                    rollSpeed = SPEED_FAST;
                    break;
                case MODERATE:
                    rollSpeed = SPEED_MODERATE;
                    break;
                case SLOW:
                    rollSpeed = SPEED_SLOW;
                    break;
                case SLOWER:
                    rollSpeed = SPEED_SLOWER;
                    break;
            }
        }

        /*
         * FUNCTION     : Window_Closing()
         * DESCRIPTION  :
         *      Occurs when the window begins to close.
         */
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //abort the thread if it's running when the window is closing
            try
            {
                rollThread.Abort();
            }
            catch { }
        }
    }
}
