using AngleSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Test_app_parcing_ABP
{
    class Program
    {

        static void Main(string[] args)
        {
            first_info();
            while (true)
            {
                string Line = Console.ReadLine().Trim().ToLower();
                switch (Line)
                {
                    case "model -c":
                        GetModelsToyotaToDBFirstRecords();
                        break;
                    case "basepoolbymodel -c":
                        GetBasePoolToDBFirstRecords();
                        break;
                    case "modeldata -sh":
                        showModelsdata();
                        break;
                    default:
                        Console.WriteLine("No no no");
                        break;

                }
                
            }

            //block reverse date for url string Task2 develpment
            /*string startDate = "10.1982";
            string endDate = "10.1986";
            Console.WriteLine(string.Join("", startDate.Split('.').Reverse()));*/

        }
        public static HttpClient getHttpClient()
        {
            //request headers add here
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "Yandex");
            return client;
        }
        public static async void GetModelsToyotaToDBFirstRecords()
        {
            //don`t auto increment id and I am so tired, 
            //that the worst 
            int IDincr = 0;
            //that the worst

            HttpClient client = getHttpClient();
            var config = Configuration.Default.With(new HttpClientRequester(client))
                                              .WithDefaultLoader();
            //comment is for develpment
            var address = "https://www.ilcats.ru/toyota/?function=getModels&market=EU";

            //string html = File.ReadAllText(@"C:\Users\Пользователь\source\repos\Test_app_parcing_ABP\Test_app_parcing_ABP\html_response\keys1_toyota.html");
            var context = BrowsingContext.New(config);

            Console.WriteLine("Send Request to www.ilcats.ru getModels");
            //var document = await context.OpenAsync(req => req.Content(html));
            var document = await context.OpenAsync(address);

            //parse ready html
            Console.WriteLine("Start parse page");
            var ListStory = document.QuerySelectorAll("div.List.Multilist > div");

            var names = document.QuerySelectorAll("div.name");

            
            //get divs and entry in him
            for (int i = 0; i < names.Length; i++)
            {
                
                var ids = ListStory[i].QuerySelectorAll("div.id");
                var dates = ListStory[i].QuerySelectorAll("div.dateRange");
                var complectations = ListStory[i].QuerySelectorAll("div.modelCode");
                Database_test_scrapy_ABPEntities db = new Database_test_scrapy_ABPEntities();
                for (int j = 0; j < ids.Length; j++)
                {
                    //We need two date for next scraping url(complectation)
                    string[] date = dates[j].TextContent.Split('-');
                    //.Trim() because,  date have specific char in html content
                    Console.WriteLine("name:" + names[i].TextContent +
                        "id:" + ids[j].TextContent + " startDate" + date[0].Trim() + " endDate" + date[1].Trim() +
                        "complectation:" + complectations[j].TextContent);

                    
                    Model car = new Model();
                    car.IDModel = IDincr;
                    car.name = names[i].TextContent;
                    car.startDate = date[0].Trim();
                    if (date[1].Trim() != "...")
                    {
                        car.endDate = date[1].Trim();
                    }

                    car.complectation = complectations[j].TextContent;
                    car.code = ids[j].TextContent;
                    db.Model.Add(car);
                    IDincr++;
                }
                db.SaveChanges();
            }
            
            Console.WriteLine("insert first date to DB");
        }
        public static async void GetBasePoolToDBFirstRecords()
        {
            //don`t auto increment id and I am so tired, 
            //that the worst 
            int IDincr = 0;
            //that the worst

            HttpClient client = getHttpClient();
            var config = Configuration.Default.With(new HttpClientRequester(client))
                                              .WithDefaultLoader();
            //comment is work with http conection
            var address = "https://www.ilcats.ru/toyota/?function=getComplectations&market=EU&model=281220&startDate=198210&endDate=198610";

            //string html = File.ReadAllText(@"C:\Users\Пользователь\source\repos\Test_app_parcing_ABP\Test_app_parcing_ABP\html_response\keys2_toyota.html");
            var context = BrowsingContext.New(config);
            Console.WriteLine("Send Request to www.ilcats.ru get Base Pool of complectation");
            var document = await context.OpenAsync(address);
            //var document = await context.OpenAsync(req => req.Content(html));

            //parse ready html
            var ListStory = document.QuerySelector("table > tbody");

            //get first tr for th table
            var first_tr_table = ListStory.QuerySelector("tr");
            var ths_table = first_tr_table.QuerySelectorAll("th");

            IDictionary<string, string> BaseComplectPool = new Dictionary<string, string>();
            
            // 2 use for parsing our values in table
            for (int i = 2; i < ths_table.Length; i++)
            {
                BaseComplectPool[ths_table[i].TextContent] = "";
                Console.WriteLine(ths_table[i].TextContent);
            }
            string[] myKeys;
            myKeys = BaseComplectPool.Keys.ToArray();

            List<Dictionary<string, string>> baseJSON = new List<Dictionary<string, string>>();

            var all_tr_table = ListStory.QuerySelectorAll("tr");
            /*table>tbody>tr>td 
             * i=1 without first tr because td is my keys
             * j=2 because our value there
            */
            Database_test_scrapy_ABPEntities db = new Database_test_scrapy_ABPEntities();
            //all_tr_table.Length 
            for (int i = 1; i< all_tr_table.Length; i++)
            {
                var trs = all_tr_table[i].QuerySelectorAll("td");
                int myKeysIndex = 0;
                for (int j=2; j<trs.Length;j++) {
                    BaseComplectPool[myKeys[myKeysIndex]] = trs[j].TextContent;
                    myKeysIndex++;
                }
                baseJSON.Add(new Dictionary<string, string>(BaseComplectPool));

                Console.WriteLine("Комплектация:" + trs[0].TextContent +
                        "baseJSON:" + string.Join(Environment.NewLine, JsonSerializer.Serialize(baseJSON)));

                BasePool bpool = new BasePool();
                bpool.Id = IDincr;
                bpool.code = 12;
                bpool.baseJSON = string.Join(Environment.NewLine, JsonSerializer.Serialize(baseJSON));
                bpool.complectation = trs[0].TextContent;
                IDincr++;
                db.SaveChanges();
            }
            
        }
        public static async void showModelsdata()
        {
            Database_test_scrapy_ABPEntities _context = new Database_test_scrapy_ABPEntities();
            var listofData = _context.Model.ToList();
            Console.WriteLine(listofData.Count.ToString());
            for (int i = 0; i < listofData.Count; i++)
            {
                if (listofData[i].endDate == null)
                {
                    Console.WriteLine("name:" + listofData[i].name +
                        "id:" + listofData[i].code + " startDate" + listofData[i].startDate + 
                        "modelCode:" + listofData[i].complectation);
                    continue;
                }
                Console.WriteLine("name:" + listofData[i].name +
                        "id:" + listofData[i].code + " startDate" + listofData[i].startDate + " endDate" + listofData[i].startDate +
                        "modelCode:" + listofData[i].complectation);
            }
        }
        private static void first_info()
        {
            Console.WriteLine("Hi");
            Console.WriteLine("That`s simple raw product");
            Console.WriteLine("Functional is insert one model.Task1 example");
            Console.WriteLine("And only write data to console.Task2 example");
            Console.WriteLine("And only one way.");
            Console.WriteLine("model -c + Enter");
            Console.WriteLine("BasePoolbyModel -c + Enter");
            Console.WriteLine("modeldata -sh + Enter");
            Console.WriteLine("that's all I managed:(");
            Console.WriteLine("if used develpment, need change path to file html");
        }
    }
}
