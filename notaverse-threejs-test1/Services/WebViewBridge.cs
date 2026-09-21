using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Web.WebView2.Wpf;
using System.Text.Json;

namespace notaverse_threejs_test1.Services
{
    /// <summary>
    /// Обёртка над WebView2 для вызова JS-функций с корректной
    /// сериализацией аргументов.
    /// </summary>
    public class WebViewBridge
    {
        private readonly WebView2 _webView;

        public WebViewBridge(WebView2 webView)
        {
            _webView = webView;
        }

        /// <summary>
        /// Вызов JS-функции без аргументов.
        /// </summary>
        public async Task CallAsync(string functionName)
        {
            var script = $"{functionName}();";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Вызов JS-функции с одним строковым аргументом.
        /// </summary>
        public async Task CallAsync(string functionName, string arg)
        {
            var json = JsonSerializer.Serialize(arg);
            var script = $"{functionName}({json});";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Вызов JS-функции с двумя строковыми аргументами.
        /// </summary>
        public async Task CallAsync(string functionName, string arg1, string arg2)
        {
            var json1 = JsonSerializer.Serialize(arg1);
            var json2 = JsonSerializer.Serialize(arg2);
            var script = $"{functionName}({json1}, {json2});";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Вызов JS-функции с произвольным объектом (сериализуется в JSON).
        /// </summary>
        public async Task CallWithJsonAsync(string functionName, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var script = $"{functionName}({json});";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Вызов JS-функции с Base64-строкой (спец. для передачи больших данных).
        /// </summary>
        public async Task CallWithBase64Async(string functionName, string base64Data, string fileName)
        {
            // Для Base64 используем Uint8Array в JS, чтобы не тянуть всю строку как JS-литерал
            var script = $@"
                (function() {{
                    const binary = atob('{base64Data}');
                    const bytes = new Uint8Array(binary.length);
                    for (let i = 0; i < binary.length; i++) {{
                        bytes[i] = binary.charCodeAt(i);
                    }}
                    {functionName}(bytes, {JsonSerializer.Serialize(fileName)});
                }})();";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }
}
