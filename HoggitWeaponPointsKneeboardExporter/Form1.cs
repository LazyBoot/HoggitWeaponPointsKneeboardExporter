using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/* Welcome to Hoggit Weapon Points Kneebaord Exporter (wip)
 * This program is designed to assist in the semi-automated process
 * of creating and maintaining the Weapon Points kneeboard pages.
 * 
 * This project was started on 11NOV2020 via a convo on the Hoggit Discord.
 * 
 * Instructions: 
 * Configure the File Locations and exports. (Find by searching "Configure This")
 * Ensure you have a source image selected
 * Ensure you have a json selected
 * Ensure you have chosen the name of your exports
 * Run
 * Done
 * 
 * Json source: https://gitlab.com/LazyBoot/jsontest
 */

namespace HoggitWeaponPointsKneeboardExporter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Configure This
            string fileLocation = @"C:\...\Projects\Hoggit Kneeboard Generator\";//this is the directory where all the files are. edit out personal info
            int pageNumber = 1;//this is the first page number for the kneeboard

            //https://stackoverflow.com/questions/24174823/how-to-draw-a-part-of-a-png-image-c-sharp
            //https://stackoverflow.com/questions/10658994/using-graphics-drawimage-to-draw-image-with-transparency-alpha-channel

            //Configure This
            //Font tahomaFont = new Font("Tahoma", 30);//this is the font I designed this with. title was 40. everythign else was 30
            //Font chosenFont_notTitle = new Font("Tahoma", 30);
            Font chosenFont_Title = new Font("Tox Typewriter", 40, FontStyle.Bold);//your system may not have this. It is nice though
            Font chosenFont_notTitle = new Font("Tox Typewriter", 30, FontStyle.Bold);//your system may not have this. It is nice though

            //Configure This based on the font
            int imageWidth = 682;//width of the backgorund png
            int imageHeight = 1024;//height of the background png
            int weaponNameMargin = 25;//the number of pixels for the left margin
            int weaponFontSize = 30;//font size
            int titleUpperMargin = 50;//the number of pixels for the top margin of the title
            int gridWriteLine = 143;//where the first weapon entry sits. this may chagne slightly based on font and font size
            int entryVerticalSpacing = 47;//the spacing between the weapon entries. this changes based on font and font size

            int titleTextMargin = imageWidth / 2 - 260;//the smart way to do this is to link them to the width of the image and center it
            int airWeaponsTextMargin = 180;//the smart way to do this is to link them to the width of the image and center it
            int groundWeaponsTextMargin = 170;//the smart way to do this is to link them to the width of the image and center it

            //Configure This
            var json = File.ReadAllText(fileLocation + @"weapon_points.json");//read the json file into memory
            var weaps = Weapons.FromJson(json);//idk what this actually does

            json.Replace('_', '-');//replaces the underscores with dashes. This does not work.

            //Configure This
            using (var src = new Bitmap(fileLocation + @"kneepad.png"))//this is the background image location. DCS likes 4:6 kneebaord ratio
            using (var bmp = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppPArgb))//sets the drawing area (?)
            using (var gr = Graphics.FromImage(bmp))//time to draw
            {
                //----A2A pages----//

                List<string> A2A_weapons_and_points_array = new List<string>();//make an empty string
                //and then put the contents of the Air entries in the string

                foreach (var weapon in weaps.Where(w => w.Value.Category == "Air"))
                {
                    string weaponCombo = $"{weapon.Key}: {weapon.Value.Points}" + " points";
                    A2A_weapons_and_points_array.Add(weaponCombo);
                }

                int numberOfAirWeaponEntries = A2A_weapons_and_points_array.Count();//makes an int to help determine stuff later
                string exportedPagefilename = "hoggitWeaponPoints" + pageNumber + ".png";//maing sure the page number stuff is set...i think

                while (A2A_weapons_and_points_array.Count > 0)//while there is stuff in the array
                {
                    //--Init a Fresh Page--//
                    gr.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height));//this "clears" the area by pasting the source image on top
                    gr.DrawString("Hoggit Loadout Points", chosenFont_Title, Brushes.Black, new PointF(titleTextMargin, titleUpperMargin));//title

                    //--reset the line variable for the new page--//
                    gridWriteLine = 143;//you should change the way this is done, maybe. it is where the first weapon entry sits

                    //sub-title and spaceing afterwards
                    gr.DrawString("Air Weapons", chosenFont_notTitle, Brushes.Black, new PointF(weaponNameMargin + airWeaponsTextMargin, gridWriteLine));
                    gridWriteLine = gridWriteLine + entryVerticalSpacing;

                    //the max I would like to have on a page is 17 entries

                    if (numberOfAirWeaponEntries <= 17)//if there is anywhere from 1 to 17 entries
                    {
                        foreach (string weaponEntry in A2A_weapons_and_points_array)//write an entry, go to the next line, then write another entry, etc
                        {
                            gr.DrawString(weaponEntry, chosenFont_notTitle, Brushes.Black, new PointF(weaponNameMargin, gridWriteLine));
                            gridWriteLine = gridWriteLine + entryVerticalSpacing;
                        }
                        A2A_weapons_and_points_array.RemoveRange(0, A2A_weapons_and_points_array.Count);//this happens to remove the rest of the array entries so that
                                                                                                        //the while loop fails and quits and the program ends
                    }
                    else if (numberOfAirWeaponEntries > 17)//if there are more than 17 more entries, you know that you will need another page
                    {
                        for (int i = 0; i < 17; i++)//do this 17 times, 1 time for each weapon
                        {
                            gr.DrawString(A2A_weapons_and_points_array[i], chosenFont_notTitle, Brushes.Black, new PointF(weaponNameMargin, gridWriteLine));
                            gridWriteLine = gridWriteLine + entryVerticalSpacing;
                        }
                        A2A_weapons_and_points_array.RemoveRange(0, 17);//removes the 17 weapons from the array we just wrote in
                        numberOfAirWeaponEntries = numberOfAirWeaponEntries - 17;//modifies the helper int too
                    }
                    //this happens after 17 (or less) entries have been entered
                    pageNumber = pageNumber + 1;//adds 1 to the page number
                    //Configure This
                    bmp.Save(fileLocation + exportedPagefilename, ImageFormat.Png);//saves the file as a png
                    exportedPagefilename = "hoggitWeaponPoints" + pageNumber + ".png";//this updates the pageNumber variable. Necessary bc it is nested in another variable
                }//this will loop if there is still something in the weapon array, making more pages


                //----A2G pages----//
                //this is almost exactly like the above part, except for the A2G weapons

                List<string> A2G_weapons_and_points_array = new List<string>();//make an empty string
                //and then put the contents of the Ground entries in the string
                foreach (var weapon in weaps.Where(w => w.Value.Category == "Ground"))
                {
                    string weaponCombo = $"{weapon.Key}: {weapon.Value.Points}" + " points";
                    A2G_weapons_and_points_array.Add(weaponCombo);
                }
                int numberOfGroundWeaponEntries = A2G_weapons_and_points_array.Count();
                
                exportedPagefilename = "hoggitWeaponPoints" + pageNumber + ".png";

                while (A2G_weapons_and_points_array.Count > 0)
                {

                //--Init a Fresh Page--//
                gr.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height));//this "clears" the area by pasting the source image again
                gr.DrawString("Hoggit Loadout Points", chosenFont_Title, Brushes.Black, new PointF(titleTextMargin, titleUpperMargin));//title

                //--reset the line variable for the new page--//
                gridWriteLine = 143;//you should change the way this is done, maybe. it is where the first weapon entry sits

                    //sub-title and spaceing afterwards
                    gr.DrawString("Ground Weapons", chosenFont_notTitle, Brushes.Black, new PointF(weaponNameMargin + groundWeaponsTextMargin, gridWriteLine));
                gridWriteLine = gridWriteLine + entryVerticalSpacing;

                //the max I would like to have on a page is 17 entries

                if (numberOfGroundWeaponEntries <= 17)
                {
                    foreach (string weaponEntry in A2G_weapons_and_points_array)
                    {
                        gr.DrawString(weaponEntry, chosenFont_notTitle, Brushes.Black, new PointF(weaponNameMargin, gridWriteLine));
                        gridWriteLine = gridWriteLine + entryVerticalSpacing;
                    }
                        A2G_weapons_and_points_array.RemoveRange(0, A2G_weapons_and_points_array.Count);//this happens to remove the rest of the array entries so that
                        //the while loop fails and quits
                    }
                else if (numberOfGroundWeaponEntries > 17)//if there are more than 17 more entries, you know that you will need another page
                {
                        for (int i = 0; i < 17; i++)//do this 17 times, 1 for each munition
                        {
                        gr.DrawString(A2G_weapons_and_points_array[i], chosenFont_notTitle, Brushes.Black, new PointF(weaponNameMargin, gridWriteLine));
                        gridWriteLine = gridWriteLine + entryVerticalSpacing;
                        }
                        A2G_weapons_and_points_array.RemoveRange(0,17);//removes the 17 weapons we just wrote in
                        numberOfGroundWeaponEntries = numberOfGroundWeaponEntries - 17;//modifies the helper int too
                    }
                    pageNumber = pageNumber + 1;//adds 1 to the page number
                    //Configure This
                    bmp.Save(fileLocation + exportedPagefilename, ImageFormat.Png);//saves the file as a png
                    exportedPagefilename = "hoggitWeaponPoints" + pageNumber + ".png";//this updates the pageNumber variable. Necessary bc it is nested in another variable
                }
            }
            //Configure This if you want
            //Process.Start(fileLocation + @"hoggitWeaponPoints.png");//opens the indicated file in your default image viewer program
            Application.Exit();//bc some ppl despise WinForm
        }
    }
}
