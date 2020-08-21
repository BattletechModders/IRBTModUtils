namespace IRBTModUtils {
    
    public class ModStats {
    }

    public class ModConfig {

        public bool Debug = true;
        public bool Trace = false;

        public class DialogueOptions {
            public string[] Portraits = {
                    "sprites/Portraits/guiTxrPort_DEST_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_01_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_02_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_03_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_04_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_05_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_06_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_07_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_08_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_09_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_10_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_11_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_12_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_davion_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_default_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_kurita_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_liao_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_marik_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_steiner_utr.png",
            };
            public string CallsignsPath = "BattleTech_Data/StreamingAssets/data/nameLists/name_callsign.txt";
        }
        public DialogueOptions Dialogue = new DialogueOptions();

        public void LogConfig() {
            Mod.Log.Info?.Write("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info?.Write($"  DEBUG: {this.Debug} Trace: {this.Trace}");
            Mod.Log.Info?.Write("=== DIALOGUE OPTIONS ===");
        }
    }
}
