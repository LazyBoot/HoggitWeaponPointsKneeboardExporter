using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HoggitWeaponPointsKneeboardExporterConsole
{
    class Program
    {
        //Configure This based on the font
        const int MaxWeaponsPerPage = 17;
        const int imageWidth = 682;//width of the backgorund png
        const int imageHeight = 1024;//height of the background png
        const int weaponNameMargin = 25;//the number of pixels for the left margin
        const int weaponFontSize = 30;//font size
        const int titleUpperMargin = 50;//the number of pixels for the top margin of the title
        const int firstWeaponLine = 143;//where the first weapon entry sits. this may chagne slightly based on font and font size
        const int entryVerticalSpacing = 47;//the spacing between the weapon entries. this changes based on font and font size

        const int titleTextMargin = imageWidth / 2 - 260;//the smart way to do this is to link them to the width of the image and center it
        const int airWeaponsTextMargin = 180;//the smart way to do this is to link them to the width of the image and center it
        const int groundWeaponsTextMargin = 170;//the smart way to do this is to link them to the width of the image and center it


        private static string CurrentPath;

        internal static Font chosenFont_Title;
        internal static Font chosenFont_notTitle;

        static void Main(FileInfo inputFile, string outputFolder)
        {
            if (inputFile == null || string.IsNullOrWhiteSpace(outputFolder))
            {
                Console.WriteLine("Use --help for parameters");
                return;
            }

            CurrentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            DoTheThing(inputFile, outputFolder);
        }



        static void DoTheThing(FileInfo intputFile, string outputFolder)
        {
            //Configure This
            //string fileLocation = @"C:\...\Projects\Hoggit Kneeboard Generator\";//this is the directory where all the files are. edit out personal info
            int pageNumber = 1;//this is the first page number for the kneeboard

            //https://stackoverflow.com/questions/24174823/how-to-draw-a-part-of-a-png-image-c-sharp
            //https://stackoverflow.com/questions/10658994/using-graphics-drawimage-to-draw-image-with-transparency-alpha-channel

            //Configure This
            //Font tahomaFont = new Font("Tahoma", 30);//this is the font I designed this with. title was 40. everythign else was 30
            //Font chosenFont_notTitle = new Font("Tahoma", 30);
            
            var privateFonts = new System.Drawing.Text.PrivateFontCollection();
            privateFonts.AddFontFile(Path.Combine(CurrentPath, @"Tox Typewriter.ttf"));

            chosenFont_Title = new Font(privateFonts.Families[0], 40, FontStyle.Bold);//your system may not have this. It is nice though
            chosenFont_notTitle = new Font(privateFonts.Families[0], 30, FontStyle.Bold);//your system may not have this. It is nice though




            //Configure This
            var json = File.ReadAllText(intputFile.ToString());//read the json file into memory
            var weaps = Weapons.FromJson(json.Replace('_', '-'));//idk what this actually does


            //Configure This
            using (var src = new Bitmap(Path.Combine(CurrentPath, @"kneepad.png")))//this is the background image location. DCS likes 4:6 kneeboard ratio
            using (var bmp = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppPArgb))//sets the drawing area (?)
            using (var gr = Graphics.FromImage(bmp))//time to draw
            {
                foreach (var category in weaps.GroupBy(w => w.Value.Category).Select(c => c.First().Value.Category))
                {
                    CreatePages(outputFolder, ref pageNumber, weaps.Where(w => w.Value.Category == category), category, src, bmp, gr);
                }
            }
        }

        private static void CreatePages(string outputFolder, ref int pageNumber, IEnumerable<KeyValuePair<string, Weapons>> weaps, string category, Bitmap src, Bitmap bmp, Graphics gr)
        {
            var textMargin = category == "Ground" ? groundWeaponsTextMargin : airWeaponsTextMargin;

            List<string> weapons_and_points_array = new List<string>();//make an empty string
                                                                           //and then put the contents of the entries in the string

            foreach (var weapon in weaps)
            {
                string weaponCombo = $"{weapon.Key}:   {Math.Round(weapon.Value.Points, 1)}" + " points";
                weapons_and_points_array.Add(weaponCombo);
            }

            int numberOfAirWeaponEntries = weapons_and_points_array.Count();//makes an int to help determine stuff later
            string exportedPagefilename = "hoggitWeaponPoints" + pageNumber + ".jpg";//maing sure the page number stuff is set...i think

            while (weapons_and_points_array.Count > 0)//while there is stuff in the array
            {
                //--Init a Fresh Page--//
                gr.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height));//this "clears" the area by pasting the source image on top
                gr.DrawString("Hoggit Loadout Points", chosenFont_Title, Brushes.Black, new PointF(titleTextMargin, titleUpperMargin));//title

                //--reset the line variable for the new page--//
                var gridWriteLine = firstWeaponLine;//you should change the way this is done, maybe. it is where the first weapon entry sits

                //sub-title and spaceing afterwards
                gr.DrawString($"{category} Weapons", chosenFont_notTitle, Brushes.Black, new PointF(weaponNameMargin + textMargin, gridWriteLine));
                gridWriteLine = gridWriteLine + entryVerticalSpacing;

                //the max I would like to have on a page is 17 entries

                if (numberOfAirWeaponEntries <= MaxWeaponsPerPage)//if there is anywhere from 1 to 17 entries
                {
                    foreach (string weaponEntry in weapons_and_points_array)//write an entry, go to the next line, then write another entry, etc
                    {
                        gr.DrawString(weaponEntry, chosenFont_notTitle, Brushes.Black, new PointF(weaponNameMargin, gridWriteLine));
                        gridWriteLine = gridWriteLine + entryVerticalSpacing;
                    }
                    weapons_and_points_array.RemoveRange(0, weapons_and_points_array.Count);//this happens to remove the rest of the array entries so that
                                                                                                    //the while loop fails and quits and the program ends
                }
                else if (numberOfAirWeaponEntries > MaxWeaponsPerPage)//if there are more than 17 more entries, you know that you will need another page
                {
                    for (int i = 0; i < MaxWeaponsPerPage; i++)//do this 17 times, 1 time for each weapon
                    {
                        gr.DrawString(weapons_and_points_array[i], chosenFont_notTitle, Brushes.Black, new PointF(weaponNameMargin, gridWriteLine));
                        gridWriteLine = gridWriteLine + entryVerticalSpacing;
                    }
                    weapons_and_points_array.RemoveRange(0, MaxWeaponsPerPage);//removes the 17 weapons from the array we just wrote in
                    numberOfAirWeaponEntries = numberOfAirWeaponEntries - MaxWeaponsPerPage;//modifies the helper int too
                }
                //this happens after 17 (or less) entries have been entered
                pageNumber = pageNumber + 1;//adds 1 to the page number
                                            //Configure This
                bmp.Save(Path.Combine(outputFolder, exportedPagefilename), ImageFormat.Jpeg);//saves the file as a png
                exportedPagefilename = "hoggitWeaponPoints" + pageNumber + ".jpg";//this updates the pageNumber variable. Necessary bc it is nested in another variable
            }//this will loop if there is still something in the weapon array, making more pages
        }
    }
}
