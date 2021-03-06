﻿using System;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace VpxEncode
{
  static class StringBuilderExtension
  {
    static ConcurrentDictionary<WeakReference<StringBuilder>, bool> prev = new ConcurrentDictionary<WeakReference<StringBuilder>, bool>();

    public static StringBuilder AppendIfPrev(this StringBuilder sb, string value)
    {
      StringBuilder mapped = null;
      var pair = prev.FirstOrDefault(x => x.Key.TryGetTarget(out mapped) && mapped == sb);
      if (pair.Key != null && pair.Value)
        sb.Append(value);
      return sb;
    }

    public static StringBuilder AppendForPrev(this StringBuilder sb, string value)
    {
      bool p = !string.IsNullOrWhiteSpace(value);
      if (p)
        sb.Append(value);
      StringBuilder mapped = null;
      var pair = prev.FirstOrDefault(x => x.Key.TryGetTarget(out mapped) && mapped == sb);
      if (pair.Key == null)
        prev[new WeakReference<StringBuilder>(sb)] = p;
      else
        prev[pair.Key] = p;
      return sb;
    }

    public static bool Contains(this StringBuilder sb, string value)
    {
      if (string.IsNullOrEmpty(value))
        return false;

      int valueIndex = 0;
      for (int i = 0; i < sb.Length; i++)
      {
        if (sb[i] == value[valueIndex])
        {
          if (++valueIndex >= value.Length)
            return true;
        }
        else
          valueIndex = 0;
      }
      return false;
    }

    public static void TrimSpaces(this StringBuilder sb)
    {
      sb.Replace("  ", " ");
      sb.Replace("   ", " ");
    }
  }
}
