using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ArrayExtension
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// 返回一个随机的值
    /// </summary>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException">若数组为空会抛出IndexOutOfRangeException</exception>
    public static T Random<T>(this T[] array) => array[_random.Next(array.Length)];
}
