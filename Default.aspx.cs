using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Task_2
{

    public partial class Default : System.Web.UI.Page
    {
        // Title variables for the table cells.
        const int ALBUM = 0, ARTIST = 1, DATE = 2, TRACKS = 3, POPULARITY = 4;

        // Authorization key ... at the moment needs to be manually reset every 3600 seconds.
        // To get a new token go here https://beta.developer.spotify.com/console/get-search-item/
        // scroll down and go to OAuth Token then press Get Token. After accepting this will generate a new
        // token code. Copy and replace that token code into here. Unforunately I was unable to get the refresh
        // token to work as expected.
        const string token = "BQAdFDzrPcE-_AXGtaOQUrDyGoxsnClCiJahKOWXOpbzt78UwmyVEYCeXfbo6GqYUJrVdcm_Hbz1FZrGpwhZeBxENpFrwp4gd7JlbBFZpy78uY8IXZdRkZH2zbd677QGRBq_EbEY3TvmnUg";



        // Event function that triggers after the search button is hit.
        public void button1Clicked(object sender, EventArgs args)
        {
            // Search button and actions
            button1.Text = "Search";
            string name = query.Text;
            // Error checking with try/catch
            try
            {

                // sets label1 to not be visible if results are found. *Label1 says "no results are found"
                Label1.Visible = false;
                Label2.Visible = false;

                // ensures that a name is entered
                if (name != null)
                {
                    

                    // creates the search request from spotify. Telling it where to call from.
                    string search = "https://api.spotify.com/v1/search?q=" + name + "&type=artist";
                    var request = (HttpWebRequest)WebRequest.Create(search);
                    request.Method = "GET";
                    request.ContentType = "application/json";
                    request.Accept = "application/json";
                    request.Headers.Add("Authorization", "Bearer " + token);

                    // The output of the response stream.
                    string output = String.Empty;

                    // Gets a response from the request call and stores it into output
                    // Notice how I close the response and abort the request. Release is needed for future WebRequests  
                    using (var response = request.GetResponse())
                    {
                        using (var stream = new StreamReader(response.GetResponseStream()))
                        {
                            output = stream.ReadToEnd();
                        }
                        response.Close();
                    }
                    request.Abort();

                    // Used for the next api request
                    // Gets the first id from the output stream.
                    string idString = "";
                    int index1 = output.IndexOf("id\" : \"");
                    if (index1 != -1) idString = output.Substring(index1 + 7, 22);

                    // Used to build the table.
                    TableRow[] tRow = new TableRow[6];
                    TableCell[][] tCell = new TableCell[6][];

                    // Builds an empty table
                    for (int i = 0; i < 6; i++)
                    {
                        tRow[i] = new TableRow();
                        tCell[i] = new TableCell[6];
                        for (int j = 0; j < 5; j++)
                        {
                            tCell[i][j] = new TableCell();
                            tCell[i][j].BorderStyle = BorderStyle.Solid;
                            tCell[i][j].Text = "";
                            tCell[i][j].Visible = true;
                            tRow[i].Cells.Add(tCell[i][j]);
                        }
                        Table1.Rows.Add(tRow[i]);
                        Table1.Visible = true;
                    }

                    // Fills in the artist name in the table.
                    for (int i = 1; i < 6; i++)
                    {
                        tCell[i][ARTIST].Text = name;
                    }

                    // Fills in the header row with titles.
                    Table1.Visible = true;
                    tCell[0][ALBUM].Text = "Album Name";
                    tCell[0][ARTIST].Text = "Artist Name";
                    tCell[0][DATE].Text = "Date of Release";
                    tCell[0][TRACKS].Text = "Tracks";
                    tCell[0][POPULARITY].Text = "Popularity";

                    // Creates the albumb search request from spotify using ID from search artist output.
                    string id = "https://api.spotify.com/v1/artists/" + idString + "/albums";
                    var idRequest = (HttpWebRequest)WebRequest.Create(id);
                    idRequest.Method = "GET";
                    idRequest.ContentType = "application/json";
                    idRequest.Accept = "application/json";
                    idRequest.Headers.Add("Authorization", "Bearer " + token);

                    // The output of the response stream.
                    string output2 = String.Empty;

                    // Gets a response from the request call and stores it into output
                    using (var response2 = idRequest.GetResponse())
                    {
                        using (var stream2 = new StreamReader(response2.GetResponseStream()))
                        {
                            output2 = stream2.ReadToEnd();
                        }
                        response2.Close();
                    }
                    idRequest.Abort();

                    // Array of album names.
                    string[] albumName = new string[5];

                    // Used for traversing the output stream.
                    index1 = 0;
                    int endIndex = 0;

                    // Fills in the album name column in table1 from the output stream.
                    for (int i = 0, startIndex = 0; i < 5; i++)
                    {
                        albumName[i] = "";
                        index1 = output2.IndexOf(" \"name\" : \"", startIndex);
                        if (index1 == -1) break;// in case it doesn't find the substring
                        else
                        {
                            // uses index start and stop points to retreive substring
                            // " "name" : "" has 11 characters
                            endIndex = output2.IndexOf("\"", index1 + 11);
                            albumName[i] = output2.Substring(index1 + 11, endIndex - (index1 + 11));

                            // Gets rid of the excessive data that is not relevant to intended search results.
                            // the output includes Artist name and the literal "Various Artists" after " "name" : ""
                            if (albumName[i] != name && albumName[i] != "Various Artists")
                            {
                                tCell[i + 1][ALBUM].Text = albumName[i];
                            }
                            // ensures the loop counter doesn't get wasted on unintended Artist name or "Various Artists"
                            else
                            {
                                i--;
                            }
                        }
                        //starts index pointer to right after the found substring
                        startIndex = index1 + (endIndex - index1);
                    }

                    // fills in the release date column; can use the same output as previous request response
                    // uses the same logic as previous substring search; in hindsight, should have made a function for this :P
                    string[] releaseDate = new string[5];
                    index1 = 0;
                    for (int i = 0, startIndex = 0; i < 5; i++)
                    {
                        releaseDate[i] = "";
                        index1 = output2.IndexOf(" \"release_date\" : \"", startIndex);
                        if (index1 == -1) break;
                        else
                        {
                            endIndex = output2.IndexOf("\"", index1 + 19);
                            releaseDate[i] = output2.Substring(index1 + 19, endIndex - (index1 + 19));
                            tCell[i + 1][DATE].Text = releaseDate[i];
                        }
                        startIndex = index1 + (endIndex - index1);
                    }

                    // again, could have used a similar function as above
                    // this stores the first 5 album IDs used for later WebRequests
                    string[] albumID = new string[5];
                    index1 = 0;
                    for (int i = 0, startIndex = 0; i < 5; i++)
                    {
                        albumID[i] = "";
                        index1 = output2.IndexOf("https://open.spotify.com/album/", startIndex);
                        if (index1 == -1) break;
                        else
                        {
                            endIndex = output2.IndexOf("\"", index1 + 31);
                            albumID[i] = output2.Substring(index1 + 31, endIndex - (index1 + 31));
                            tCell[i + 1][TRACKS].Text = albumID[i];
                        }
                        startIndex = index1 + (endIndex - index1);
                    }


                    // I noticed I can easily loop for the remaining columns
                    // the following are strings that will be used for the loop
                    string albumFiles = ""; // will combine with albumID[] for api request string
                    string output3 = ""; // will contain the response of the request
                    string[] popNum = new string[5]; // for the popularity column
                    string[] tracksNum = new string[5]; // for the tracks column
                    for (int i = 0; i < 5; i++)
                    {
                        //usual requests as before
                        albumFiles = "https://api.spotify.com/v1/albums/" + albumID[i];
                        var albumRequest = (HttpWebRequest)WebRequest.Create(albumFiles);
                        albumRequest.Method = "GET";
                        albumRequest.ContentType = "application/json";
                        albumRequest.Accept = "application/json";
                        albumRequest.Headers.Add("Authorization", "Bearer " + token);

                        // usual output taken from response as before
                        output3 = String.Empty;
                        using (var response3 = albumRequest.GetResponse())
                        {
                            using (var stream3 = new StreamReader(response3.GetResponseStream()))
                            {
                                output3 = stream3.ReadToEnd();
                            }
                            response3.Close();
                        }
                        albumRequest.Abort();

                        // fills in the popularity column
                        // I noticed popularity is only given once in the beginning of the output
                        index1 = 0;
                        popNum[i] = "";
                        index1 = output3.IndexOf("\"popularity\" : ");
                        if (index1 == -1) popNum[i] = "-1"; // just in case it wasn't popular?
                        else
                        {
                            // ""popularity" : " has 15 characters
                            endIndex = output3.IndexOf(",", index1 + 15);
                            popNum[i] = output3.Substring(index1 + 15, endIndex - (index1 + 15));
                            tCell[i + 1][POPULARITY].Text = popNum[i];
                        }

                        // fills in the tracks column
                        // I noticed the only "track_number" I needed was the last one, hence the use of LastIndexOf()
                        index1 = 0;
                        tracksNum[i] = "";
                        index1 = output3.LastIndexOf("\"track_number\" : ");
                        if (index1 == -1) tracksNum[i] = "-1";
                        else
                        {
                            endIndex = output3.IndexOf(",", index1 + 17);
                            tracksNum[i] = output3.Substring(index1 + 17, endIndex - (index1 + 17));
                            tCell[i + 1][TRACKS].Text = tracksNum[i];
                        }
                    }

                } // end of if (name != null)

            }  // end of try

            // if something goes wrong, like the search query not providing results
            // then the table gets turned off, and the "no results found" label gets turned on
            catch { 
                Table1.Visible = false;
                Label1.Text = "Sorry, no results found.";
                Label1.Visible = true;
            }
        }
    }

}
