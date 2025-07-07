using UnityEngine;

namespace Const
{
    public class stopper : MonoBehaviour
    {
        // グローバル変数として静的変数を定義
        //public static int movemode = 0;
        public static int[] capturemode = new int[100]; // 0～99
        //public static int bluemode = 0;
        public static int[] movemode = new int[100];      // 0～99
        //public static int[] capturemode = new int[100];
        public static int[] bluemode = new int[100];
        // ゲーム開始時などにmovemodeを変更する場合は、こちらで処理を追加

    }
}