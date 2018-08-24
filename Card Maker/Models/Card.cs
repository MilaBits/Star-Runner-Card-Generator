using Card_Maker.Enums;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Drawing;

namespace Card_Maker.Models {
    public class Card {

        public string Name { get; set; }
        public string Filename { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }

        public string BackgroundPath { get; set; }
        public PointF BackgroundOffset { get; set; }
        public float Scale { get; set; }

        public int Cost { get; set; }

        public Phase Phase { get; set; }

        public RollType RollType { get; set; }
        public string RollText { get; set; }
        public string RollTarget { get; set; }

        public List<Role> Roles { get; set; }

        public CritBar CritIcon { get; set; }
        public string CriticalSuccess { get; set; }
        public string CriticalFailure { get; set; }

        public Card() {
            Roles = new List<Role>();
        }

        public void SaveToJson() {
            string json = JsonConvert.SerializeObject(this);
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Cards/")) {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/Cards/");
            }

            string outputFileName = AppDomain.CurrentDomain.BaseDirectory + "/Cards/" + Filename.Split('.')[0] + ".card";

            using (FileStream fs = File.Create(outputFileName)) {
                byte[] bytes = new UTF8Encoding(true).GetBytes(json);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static Card LoadFromJson(string filename) {
            string inputFileName = AppDomain.CurrentDomain.BaseDirectory + "/Cards/" + filename + ".card";
            string json = File.ReadAllText(inputFileName.Replace("%20", " "));
            return JsonConvert.DeserializeObject<Card>(json);
        }

        public Card LoadFromJson() {
            string inputFileName = AppDomain.CurrentDomain.BaseDirectory + "/Cards/" + Filename + ".card";
            string json = File.ReadAllText(inputFileName);
            return JsonConvert.DeserializeObject<Card>(json);
        }
    }
}
