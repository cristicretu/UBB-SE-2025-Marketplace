using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace Marketplace_SE.Utilities
{
    public class FrameNavigation
    {
        public static Frame NavigateWithConstructorParameters<T>(Frame frame, params object[] constructorParams)
        where T : Page
        {
            var type = typeof(T);
            var constructorInfo = type.GetConstructor(constructorParams.Select(p => p.GetType()).ToArray());

            if (constructorInfo == null)
            {
                throw new InvalidOperationException($"No matching constructor found for {type.Name}");
            }

            var page = (Page)constructorInfo.Invoke(constructorParams);
            page.SetValue(Page.FrameProperty, frame);
            frame.Content = page;
            return frame;
        }
    }
}
