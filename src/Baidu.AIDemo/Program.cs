using System;
using System.Text;

namespace Baidu.AIDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // 设置APPID/AK/SK
            var APP_ID = "18191384";
            var API_KEY = "--";
            var SECRET_KEY = "--";

            var client = new Baidu.Aip.Nlp.Nlp(API_KEY, SECRET_KEY);
            client.Timeout = 60000;  // 修改超时时间
            

            var text = "我们知道深度学习中对图像处理应用最好的模型是CNN";

            // 调用词法分析，可能会抛出网络等异常，请使用try/catch捕获
            var result = client.LexerCustom(text);
            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
