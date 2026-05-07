using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public static class AnimationExtensions
    {
        public static Task WidthRequestTo(this VisualElement element, double newWidth, uint length, Easing easing)
        {
            double startWidth = element.WidthRequest;
            var tcs = new TaskCompletionSource<bool>();

            new Animation(v => element.WidthRequest = v, startWidth, newWidth)
                .Commit(element, "WidthRequestTo", length: length, easing: easing,
                    finished: (v, c) => tcs.SetResult(true));

            return tcs.Task;
        }
    }
}
