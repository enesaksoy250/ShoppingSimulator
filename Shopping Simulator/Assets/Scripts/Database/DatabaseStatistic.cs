using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using Unity.VisualScripting;

public class DatabaseStatistic : MonoBehaviour
{

    DatabaseReference databaseReference;
    private string userID;

    public DatabaseStatistic(DatabaseReference reference,string userID)
    {
        this.databaseReference = reference;
        this.userID = userID;

        //GetDailyRegisterDetails();
        GetDailyInfo();
        
        GetUsersWhoRemoveAds();
        GetWatchedCount(0);
        //GetPhoneAccount(); //c9912 benim tel 6f8b recep tel  1365e bilgisayar
    }


    private void GetDailyRegisterDetails()
    {
        DateTime today = DateTime.Today;
        string date = today.ToString("dd-MM-yyyy");
        string date2 = "20-04-2025"; // Bu sat�r� kald�rabilir veya bug�n�n tarihiyle de�i�tirebilirsiniz.

        databaseReference.Child("users").OrderByChild("date").EqualTo(date2).GetValueAsync().ContinueWith(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot snapshotData in snapshot.Children)
                {
                    if (snapshotData.HasChild("username") && snapshotData.HasChild("watchedAdCount") && snapshotData.HasChild("gameTime"))
                    {
                        string username = snapshotData.Child("username").Value.ToString();
                        int watchedAdCount = int.Parse(snapshotData.Child("watchedAdCount").Value.ToString());
                        float gameTime = float.Parse(snapshotData.Child("gameTime").Value.ToString());


                        print("Kullan�c� Ad�: " + username + ", �zlenen Reklam Say�s�: " + watchedAdCount + ", Oyun S�resi: " + gameTime);
                        // �sterseniz bu de�erleri bir liste veya ba�ka bir veri yap�s�nda saklayabilirsiniz.
                    }
                    else
                    {
                        Debug.LogWarning(snapshotData.Key + " alt�nda gerekli alanlardan biri bulunamad�!");
                    }
                }

                // T�m kullan�c�lar i�lendikten sonra toplam say�y� yazd�rabilirsiniz.
                print(date + " tarihinde kay�t olan ki�i say�s�= " + snapshot.ChildrenCount);
            }
            else
            {
                Debug.LogWarning("Veri �ekilirken hata olu�tu!");
            }
        });
    }


    private void GetUsersWhoRemoveAds()
    {
        databaseReference.Child("users").GetValueAsync().ContinueWith(task => { 
        
            if(task.IsCompleted)
            {
               
                DataSnapshot dataSnapshot = task.Result;
         
                List<string> usernames = new List<string>();

                foreach(DataSnapshot userSnapshot in dataSnapshot.Children)
                {
                    if (userSnapshot.HasChild("removeAd"))
                    {
                        bool removeAd = userSnapshot.Child("removeAd").Value.Equals(true);

                        if (removeAd)
                        {
                            string username = userSnapshot.Child("username").Value.ToString();
                            usernames.Add(username);
                        }
                    }     

                }

                print($"Reklam kald�ran ki�i say�s�= {usernames.Count}");

                foreach (string username in usernames)
                {
                    print("Reklam kald�ran= "+username);
                }

            }
        
        
        });
    }


    private void GetWatchedCount(int minGameTime)
    {
        databaseReference.Child("users").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    Debug.LogWarning("Kullan�c� verisi bulunamad�.");
                    return;
                }

                // Ge�ici liste olu�turuluyor
                List<(string username,int gameTime, int watchedAdCount, string date,bool removeAd,int adLoadingError)> userDataList = new List<(string,int, int, string,bool,int)>();

                foreach (DataSnapshot userSnapshot in snapshot.Children)
                {
                    if (userSnapshot.Child("gameTime").Value != null &&
                        int.TryParse(userSnapshot.Child("gameTime").Value.ToString(), out int gameTime) &&
                        gameTime > minGameTime)
                    {
                        int watchedAdCount = 0;
                        string date = "-";
                        bool removeAd=false;
                        int adLoadingErrorCount = 0;
                        string username="";

                        if (userSnapshot.Child("watchedAdCount").Value != null)
                            int.TryParse(userSnapshot.Child("watchedAdCount").Value.ToString(), out watchedAdCount);

                        if (userSnapshot.Child("date").Value != null)
                            date = userSnapshot.Child("date").Value.ToString();

                        if (userSnapshot.Child("removeAd").Value != null)
                            removeAd = Convert.ToBoolean(userSnapshot.Child("removeAd").Value);

                        if(userSnapshot.Child("username").Value != null)
                            username = userSnapshot.Child("username").Value.ToString();
                     
                        if (userSnapshot.HasChild("adLoadingErrorCount"))
                            int.TryParse(userSnapshot.Child("adLoadingErrorCount").Value.ToString(), out adLoadingErrorCount);

                        else
                            adLoadingErrorCount = -2;

                        userDataList.Add((username,gameTime, watchedAdCount, date,removeAd,adLoadingErrorCount));
                    }
                }

                // gameTime�a g�re azalan �ekilde s�rala
                userDataList = userDataList.OrderByDescending(u => u.gameTime).ToList();

                // Veriyi StringBuilder ile haz�rla
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Kullan�c� ad�\tOyun s�resi\t�zlenen reklam\tKay�t Tarihi\tReklam Kald�rma\tReklam Y�kleme Hatas�");

                foreach (var user in userDataList)
                {
                    sb.AppendLine($"{user.username}\t\t{user.gameTime}\t\t{user.watchedAdCount}\t\t{user.date}\t\t{user.removeAd}\t\t{user.adLoadingError}");
                }

                // Masa�st� yolu ve dosya yaz�m�
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, "UserAdData.txt");

                try
                {
                    File.WriteAllText(filePath, sb.ToString());
                    Debug.Log("Veriler masa�st�ne s�ral� �ekilde yaz�ld�: " + filePath);
                }
                catch (IOException ex)
                {
                    Debug.LogError("Dosya yaz�l�rken bir hata olu�tu: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("Veri �ekilemedi.");
            }
        });
    }

    private void GetPhoneAccount()
    {
        databaseReference.Child("users").GetValueAsync().ContinueWith(task => {

            if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot.Exists)
                {
                    foreach (var user in dataSnapshot.Children)
                    {
                        string id = user.Key;
                        string name = user.Child("username").Value.ToString();

                        if(name == "bilgisayar")
                        {
                            print($"Telefon ID = {id}");
                        }

                    }
                }

            }
            else
            {
                Debug.LogError("Phone Account �ekilemedi");
            }
        
        });
    }

    private void GetDailyInfo()
    {
        DateTime today = DateTime.Today;
        string date = today.ToString("dd-MM-yyyy");

        databaseReference.Child("users").GetValueAsync().ContinueWith(task => { 
        
            if(task.IsCompleted)
            {

                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {

                    int gameTime=-1;
                    int watchedAdCount=-1;
                    int adLoadingErrorCount=-1;
                    string loginDate="";
                    int totalRegister=0;
                    int totalAd=0;
                    int totalTime = 0;
                    string userId="";

                    foreach (var user in snapshot.Children)
                    {
                        string registerDate = user.Child("date").Value.ToString();

                        if(date == registerDate)
                        {
                            totalRegister++;
                            string name = user.Child("username").Value.ToString();
                            int.TryParse(user.Child("gameTime").Value.ToString(), out gameTime);
                            int.TryParse(user.Child("watchedAdCount").Value.ToString(), out watchedAdCount);
                            loginDate = user.Child("date").Value.ToString();
                            totalAd += watchedAdCount;
                            totalTime += gameTime;

                            if(user.HasChild("adLoadingErrorCount"))
                                int.TryParse(user.Child("adLoadingErrorCount").Value.ToString(),out adLoadingErrorCount);

                            userId = user.Key;

                            print($"Kullan�c� ad�={name}, Oyun S�resi={gameTime}, �zlenen reklam={watchedAdCount}, " +
                                $"Reklam y�kleme hatas�={adLoadingErrorCount}, Kay�t tarihi={loginDate}, ID={userId}");

                        }

                    }

                    print($"Bug�nk�  toplam kay�t={totalRegister}, Toplam izlenen reklam={totalAd}, Oyunda ge�irilen s�re={totalTime}");

                }

            }

            else
            {
                Debug.LogError("Veriler �ekilemedi");
            }

        
        });

    }

    
}
