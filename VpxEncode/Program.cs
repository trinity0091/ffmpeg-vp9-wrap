﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VpxEncode.Output;
using YoutubeExtractor;

namespace VpxEncode
{
  public partial class Arg
  {
    public const string TIMINGS_INDEX = "ti", FIX_SUBS = "fs",
                        SUBS_INDEX = "si", SCALE = "scale",
                        PREVIEW = "preview", PREVIEW_SOURCE = "preview_s",
                        OTHER_VIDEO = "ov", OTHER_AUDIO = "oa",
                        LIMIT = "limit", UPSCALE = "upscale",
                        FILE = "file", SUBS = "subs",
                        TIMINGS = "t", START_TIME = "ss",
                        END_TIME = "to", MAP_AUDIO = "ma",
                        GENERATE_TIMING = "gent", GENERATE_T_FILE = "genf",
                        OPUS_RATE = "or", NAME_PREFIX = "name",
                        AUDIO_FILE = "af", AUTOLIMIT = "alimit",
                        AUTOLIMIT_DELTA = "alimitD", SUBS_FIRST = "sfirst",
                        YOUTUBE = "youtube", CROP = "crop",
                        INSTALL = "install", CRF_MODE = "crf",
                        SINGLE_THREAD = "sthread", TIMINGS_DELTA = "td",
                        VORBIS = "vorb", CROP_V = "cropv";
  }

  public static class ArgList
  {
    static SortedDictionary<string, Arg> ArgsDict = new SortedDictionary<string, Arg>()
    {
      [Arg.FILE] = new Arg(Arg.FILE, null, "{string} файл"),
      [Arg.SUBS] = new Arg(Arg.SUBS, null, "{string} сабы (filepath, *.ass, same"),
      [Arg.TIMINGS] = new Arg(Arg.TIMINGS, null, "{string} файл таймингов ({00:00.000|00:00:00.000|0} {00:00.000|00:00:00.000|0}\\n)"),
      [Arg.START_TIME] = new Arg(Arg.START_TIME, "0", "{00:00.000|00:00:00.000|0} начало отрезка"),
      [Arg.END_TIME] = new Arg(Arg.END_TIME, null, "{00:00.000|00:00:00.000|0} конец отрезка"),
      [Arg.MAP_AUDIO] = new Arg(Arg.MAP_AUDIO, null, "{int} для смены аудиодорожки (эквивалент -map 0:a:{int})"),
      [Arg.SCALE] = new Arg(Arg.SCALE, "960:-1", "no|{int:int} скейл изображения (default: 960:-1)"),
      [Arg.OTHER_VIDEO] = new Arg(Arg.OTHER_VIDEO, string.Empty, "{string} доп. параметры выходного файла видео \"-qmin 30\""),
      [Arg.OTHER_AUDIO] = new Arg(Arg.OTHER_AUDIO, string.Empty, "{string} доп. параметры выходного файла аудио \"-af=volume=3\""),
      [Arg.LIMIT] = new Arg(Arg.LIMIT, "10240", "{int} лимит в KB (default: 10240)"),
      [Arg.OPUS_RATE] = new Arg(Arg.OPUS_RATE, "80", "{int} битрейт аудио (Opus) в Kbps (default: 80)"),
      [Arg.NAME_PREFIX] = new Arg(Arg.NAME_PREFIX, string.Empty, "префикс имени результата"),
      [Arg.TIMINGS_INDEX] = new Arg(Arg.TIMINGS_INDEX, null, "индекс одного или нескольких (через запятую) файлов для обработки при работе с файлом таймингов"),
      [Arg.FIX_SUBS] = new Arg(Arg.FIX_SUBS, null, "замена шрифтов в ass субтитрах на Arial (если ffmpeg не находит шрифт)", false),
      [Arg.SUBS_INDEX] = new Arg(Arg.SUBS_INDEX, null, "индекс субтитров, если в контейнере", ":si={0}"),
      [Arg.AUDIO_FILE] = new Arg(Arg.AUDIO_FILE, null, "{string} внешняя аудиодорожка"),
      [Arg.GENERATE_TIMING] = new Arg(Arg.GENERATE_TIMING, null, "сгенерировать timings.txt из ffprobe", false),
      [Arg.GENERATE_T_FILE] = new Arg(Arg.GENERATE_T_FILE, null, "имя файла при " + Arg.GENERATE_TIMING),
      [Arg.AUTOLIMIT] = new Arg(Arg.AUTOLIMIT, null, "подогнать под лимит", false),
      [Arg.AUTOLIMIT_DELTA] = new Arg(Arg.AUTOLIMIT_DELTA, "240", "{int} погрешность автоподгона в KB (default: 240)"),
      [Arg.PREVIEW] = new Arg(Arg.PREVIEW, null, "{00:00.000|00:00:00.000|0} кадр для превью"),
      [Arg.PREVIEW_SOURCE] = new Arg(Arg.PREVIEW_SOURCE, null, "{string} файл для превью, если нет, то берется из -file"),
      [Arg.YOUTUBE] = new Arg(Arg.YOUTUBE, null, "{string} ссылка на видео с ютуба"),
      [Arg.CROP] = new Arg(Arg.CROP, null, "обрезка черных полос", false),
      [Arg.INSTALL] = new Arg(Arg.INSTALL, null, "установка ffmpeg в систему (только при запуске от имени Администратора)", false),
      [Arg.CRF_MODE] = new Arg(Arg.CRF_MODE, null, "{0-63} режим качества (crf) для коротких webm (alimit и limit не действуют)"),
      [Arg.UPSCALE] = new Arg(Arg.UPSCALE, null, "разрешить апскейл видео", false),
      [Arg.SINGLE_THREAD] = new Arg(Arg.SINGLE_THREAD, null, "кодирование в 1 поток", false),
      [Arg.TIMINGS_DELTA] = new Arg(Arg.TIMINGS_DELTA, "0", "{00:00.000|00:00:00.000|0} смещение времени при кодировании из файла таймингов"),
      [Arg.VORBIS] = new Arg(Arg.VORBIS, null, "{0-10 10 - максимальное качество} использовать libvorbis с выбранным качеством"),
      [Arg.CROP_V] = new Arg(Arg.CROP_V, null, "{int:int:int:int} обрезка out_w:out_h:x:y"),
      [Arg.SUBS_FIRST] = new Arg(Arg.SUBS_FIRST, null, "накладывать сабы до скейла", false)
    };

    public static void Parse(string[] args)
    {
      if (args == null || args.Length == 0)
        return;

      Arg lastArg = null;
      bool argHasValue = true;

      foreach (string arg in args)
      {
        if (argHasValue)
        {
          // Parameter name
          string name = arg.Substring(1);

          if (ArgsDict.ContainsKey(name))
            lastArg = ArgsDict[name];
          else
          {
            lastArg = new Arg(name, null);
            ArgsDict[lastArg.Name] = lastArg;
          }
          argHasValue = !lastArg.WaitForArg;
          if (argHasValue)
            lastArg.Value = "__EMPTY__";
        }
        else
        {
          // Parameter value
          lastArg.Value = arg;
          argHasValue = true;
        }
      }
    }

    public static Arg Get(string name)
    {
      if (ArgsDict.ContainsKey(name))
        return ArgsDict[name];
      return null;
    }

    public static void WriteDescription()
    {
      foreach (var pair in ArgsDict)
        Console.WriteLine(pair.Value.FullDescription);
    }
  }

  partial class Arg
  {
    public string Name { get; private set; }
    public string Value { get; set; }
    public string Descripton { get; private set; }
    public bool WaitForArg { get; private set; }
    public string Format { get; private set; }

    public Arg(string name, string def)
    {
      Name = name;
      Value = def;
      WaitForArg = true;
    }

    public Arg(string name, string def, string desc) : this(name, def) { Descripton = desc; }

    public Arg(string name, string def, string desc, bool wait) : this(name, def, desc) { WaitForArg = wait; }

    public Arg(string name, string def, string desc, string format) : this(name, def, desc) { Format = format; }

    public static implicit operator bool (Arg arg) { return arg.Value != null; }

    public string AsString() { return Value; }

    public int AsInt() { return Int32.Parse(Value); }

    public TimeSpan AsTimeSpan() { return Program.ParseToTimespan(Value); }

    public string FullDescription { get { return String.Format("-{0}\t\t{1}", Name, Descripton); } }

    public string Command { get { if (Value != null) return String.Format(Format, Value); return String.Empty; } }
  }

  public static class Program
  {
    [STAThread]
    public static void Main(string[] args)
    {
      if (args == null || args.Length == 0)
      {
        ArgList.WriteDescription();
        return;
      }

      ArgList.Parse(args);

      if (ArgList.Get(Arg.INSTALL))
      {
        FfmpegLoader loader = new FfmpegLoader();
        Task.WaitAll(loader.Install());
        return;
      }

      if (ArgList.Get(Arg.YOUTUBE))
      {
        DownloadVideo();
        return;
      }

      string filePath = ArgList.Get(Arg.FILE).AsString();
      string subPath = ArgList.Get(Arg.SUBS).AsString();
      filePath = GetFullPath(filePath);

      if (ArgList.Get(Arg.GENERATE_TIMING))
      {
        string gentfile = null;
        if (ArgList.Get(Arg.GENERATE_T_FILE))
          gentfile = Path.Combine(GetFolder(filePath), ArgList.Get(Arg.GENERATE_T_FILE).Value);
        TimingGenerator tg = new TimingGenerator(filePath, gentfile);
        tg.Generate(true);
        return;
      }

      if (filePath.EndsWith(".webm") && ArgList.Get(Arg.PREVIEW))
      {
        GeneratePreview(filePath);
        return;
      }

      if (ArgList.Get(Arg.TIMINGS))
      {
        string[] lines = File.ReadAllLines(GetFullPath(ArgList.Get(Arg.TIMINGS).AsString()));
        ushort[] indexes = null;
        if (ArgList.Get(Arg.TIMINGS_INDEX))
          indexes = ArgList.Get(Arg.TIMINGS_INDEX).AsString().Split(',').Select(x => ushort.Parse(x)).ToArray();
        else
        {
          ushort i = 0;
          indexes = lines.Select(x => i++).ToArray();
        }
        Action<int> startEncodeTiming = (index) =>
        {
          Console.WriteLine("Start encode timing file {0} line", index);
          string[] splitted = lines[index].Split(' ');
          if (splitted.Length >= 2)
          {
            ushort crf = ArgList.Get(Arg.CRF_MODE) ? ushort.Parse(ArgList.Get(Arg.CRF_MODE).Value) : (ushort)0;
            TimeSpan start = ParseToTimespan(splitted[0]), end = ParseToTimespan(splitted[1]);
            start += ParseToTimespan(ArgList.Get(Arg.TIMINGS_DELTA).AsString());
            if (ArgList.Get(Arg.AUTOLIMIT))
              if (crf != 0)
                CrfLookupEncode(crf, (newCrf) => { return Encode(index, filePath, subPath, start, end, 0, newCrf); }, GetSizeMB);
              else
                BitrateLookupEncode((newTarget) => { return Encode(index, filePath, subPath, start, end, newTarget, 0); }, GetSizeKB);
            else
              Encode(index, filePath, subPath, start, end, ArgList.Get(Arg.LIMIT).AsInt(), 0);
          }
        };
        Parallel.ForEach(indexes, new ParallelOptions
        { MaxDegreeOfParallelism = ArgList.Get(Arg.SINGLE_THREAD) ? Math.Max(1, Environment.ProcessorCount - 1) : Math.Max(1, Environment.ProcessorCount / 2) },
          (singleIndex) =>
        {
          if (lines.Length > singleIndex && singleIndex >= 0)
            startEncodeTiming(singleIndex);
        });
      }
      else
      {
        LookupEndTime(filePath);
        if (ArgList.Get(Arg.END_TIME))
        {
          ushort crf = ArgList.Get(Arg.CRF_MODE) ? ushort.Parse(ArgList.Get(Arg.CRF_MODE).Value) : (ushort)0;
          TimeSpan start = ArgList.Get(Arg.START_TIME).AsTimeSpan(), end = ArgList.Get(Arg.END_TIME).AsTimeSpan();
          start += ParseToTimespan(ArgList.Get(Arg.TIMINGS_DELTA).AsString());
          if (ArgList.Get(Arg.AUTOLIMIT))
            if (crf != 0)
              CrfLookupEncode(crf, (newCrf) => Encode(DateTime.Now.ToFileTimeUtc(), filePath, subPath, start, end, 0, newCrf), GetSizeMB);
            else
              BitrateLookupEncode((newTarget) => Encode(DateTime.Now.ToFileTimeUtc(), filePath, subPath, start, end, newTarget, 0), GetSizeKB);
          else
            Encode(DateTime.Now.ToFileTimeUtc(), filePath, subPath, start, end, ArgList.Get(Arg.LIMIT).AsInt(), 0);
        }
      }

      MessageBox.Show("OK");
    }

    public static ushort CrfLookupEncode(ushort startCrf, Func<ushort, string> encodeFunc, Func<string, double> getSize)
    {
      // Megabytes!
      double limit = ArgList.Get(Arg.LIMIT).AsInt() / 1024d;
      double delta = ArgList.Get(Arg.AUTOLIMIT_DELTA).AsInt() / 1024d;
      LinearCrfLookup bl = new LinearCrfLookup(limit - delta / 2, startCrf);

      double size = 0;
      ushort newCrf = 0;
      while (!(limit - size < delta && size < limit))
      {
        newCrf = bl.GetTarget();
        if (newCrf == 0)
          break;
        string result = encodeFunc(newCrf);
        size = getSize(result);
        bl.AddPoint(newCrf, size);
      }
      return newCrf;
    }

    public static void BitrateLookupEncode(Func<int, string> encodeFunc, Func<string, double> getSize)
    {
      int limit = ArgList.Get(Arg.LIMIT).AsInt();
      int delta = ArgList.Get(Arg.AUTOLIMIT_DELTA).AsInt();
      LinearBitrateLookup bl = new LinearBitrateLookup(limit - delta / 2);

      int size = 0;
      while (!(limit - size < delta && size < limit))
      {
        int newTarget = bl.GetTarget();
        if (newTarget == -1)
          break;
        string result = encodeFunc(newTarget);
        size = (int)getSize(result);
        bl.AddPoint(newTarget, size);
      }
    }

    static string Encode(long i, string file, string subs, TimeSpan start, TimeSpan end, int sizeLimit, int crf)
    {
      // subs = *.ass
      if (subs != null && subs.StartsWith("*"))
        subs = file.Substring(0, file.LastIndexOf('.')) + subs.Substring(1);

      // subs = same
      if (subs == "same")
        subs = file;

      bool subsWereCopied = false;
      string subsFilename = Path.GetFileName(subs);
      if (ArgList.Get(Arg.FIX_SUBS) && SubStationAlpha.IsAcceptable(subs))
      {
        string subsNew = Path.Combine(Environment.CurrentDirectory, subsFilename);
        subsFilename += i.ToString() + "_ARIAL.ass";
        SubStationAlpha ssa = new SubStationAlpha(subs);
        ssa.ChangeFontAndSave(subsNew);
        subs = subsNew;
        subsWereCopied = true;
      }

      string code = null;
      if (i < 10000)
        code = $"{i}_{DateTime.Now.ToFileTimeUtc()}";
      else
        code = i.ToString();

      string filePath = GetFolder(file),
             webmPath = Path.Combine(filePath, $"temp_{code}.webm"),
             oggPath = Path.Combine(filePath, $"temp_{code}.ogg"),
             finalPath = Path.Combine(filePath, $"{ArgList.Get(Arg.NAME_PREFIX).AsString()}{code}.webm");

      TimeSpan timeLength = end - start;
      string startString = start.ToString("hh\\:mm\\:ss\\.fff"),
             timeLengthString = timeLength.ToString("hh\\:mm\\:ss\\.fff");

      OutputProcessor sp = new SimpleProcessor();
      ProcessingUnit pu = sp.CreateOne();

      // Audio settings
      string mapAudio = ArgList.Get(Arg.MAP_AUDIO) ? $"-map 0:a:{ArgList.Get(Arg.MAP_AUDIO).AsInt()}" : string.Empty;
      int opusRate = ArgList.Get(Arg.OPUS_RATE).AsInt();
      string audioFile = ArgList.Get(Arg.AUDIO_FILE) ? GetFullPath(ArgList.Get(Arg.AUDIO_FILE).AsString()) : file;
      string otherAudio = ArgList.Get(Arg.OTHER_AUDIO).AsString();
      int vorbis = ArgList.Get(Arg.VORBIS) ? ArgList.Get(Arg.VORBIS).AsInt() : -1;
      string codecParams = vorbis == -1 ? $"-c:a opus -b:a {opusRate}K -vbr on" : $"-c:a libvorbis -q:a {vorbis}";

      // Encode audio
      string args = $"-hide_banner -y -ss {startString} -i \"{audioFile}\" {mapAudio} -ac 2 {codecParams} -vn -sn -t {timeLengthString} {otherAudio} \"{oggPath}\"";

      // Audio cache
      Cache.ACKey aKey = new Cache.ACKey(args);
      bool aCached = Cache.Instance.CreateIfPossible(aKey, oggPath);
      if (!aCached)
      {
        ExecuteFFMPEG(args, pu);
        Cache.Instance.Save(aKey, oggPath);
      }

      // No upscale check
      string scale = ArgList.Get(Arg.SCALE).AsString();
      if (scale != "no" && !ArgList.Get(Arg.UPSCALE))
      {
        string oScale = Ffprober.Probe(file).Scale;
        string[] scaleSplit = oScale.Split('x');
        if (scaleSplit.Length == 2)
        {
          int oWidth = int.Parse(scaleSplit[0]);
          int oHeight = int.Parse(scaleSplit[1]);
          scaleSplit = scale.Split(':');
          int width = int.Parse(scaleSplit[0]);
          int height = int.Parse(scaleSplit[1]);
          if (width > oWidth || height > oHeight)
            scale = "no";
        }
      }

      // VideoFilter
      const string vfDefault = "-vf \"";
      StringBuilder vf = new StringBuilder(vfDefault);
      Action addSubs = () =>
      {
        if (subs != null)
        {
          string format = subs.EndsWith("ass") || subs.EndsWith("ssa") ? "ass='{0}'{1}" : "subtitles='{0}'{1}";
          format = string.Format(format, subs.Replace(@"\", @"\\").Replace(":", @"\:"), ArgList.Get(Arg.SUBS_INDEX).Command);
          format = string.Format(new CultureInfo("en"), "setpts=PTS+{0:0.######}/TB,{1},setpts=PTS-STARTPTS", start.TotalSeconds, format);
          vf.AppendIfPrev(",").AppendForPrev(format);
        }
      };
      Action addScale = () =>
      {
        if (scale != "no")
          vf.AppendIfPrev(",").AppendForPrev($"scale={scale}:sws_flags=lanczos");
      };

      if (ArgList.Get(Arg.CROP))
      {
        string crop = GetCrop(file, startString, timeLengthString);
        if (crop != null)
          vf.AppendIfPrev(",").AppendForPrev(crop);
        pu.Write("CROP: " + crop);
      }
      if (ArgList.Get(Arg.CROP_V))
        vf.AppendIfPrev(",").AppendForPrev("crop=" + ArgList.Get(Arg.CROP_V).AsString());
      if (ArgList.Get(Arg.SUBS_FIRST))
      {
        addSubs();
        addScale();
      }
      else
      {
        addScale();
        addSubs();
      }

      if (vf.Length == vfDefault.Length)
        vf.Clear();
      else
        vf.Append("\" ");

      // Encode 2-pass video
      StringBuilder otherVideo = new StringBuilder();
      otherVideo.AppendForPrev(ArgList.Get(Arg.OTHER_VIDEO).AsString()).AppendIfPrev(" ");

      if (crf < 4 || crf > 63)
      {
        crf = ushort.MaxValue;
        if (ArgList.Get(Arg.CRF_MODE))
          try { crf = ushort.Parse(ArgList.Get(Arg.CRF_MODE).Value); if (crf < 4 || crf > 63) crf = ushort.MaxValue; }
          catch { }
      }

      string threadSettings;
      if (ArgList.Get(Arg.SINGLE_THREAD))
        threadSettings = "-tile-columns 0 -frame-parallel 0 -threads 1 -speed 1";
      else
        threadSettings = $"-tile-columns {Environment.ProcessorCount} -frame-parallel 1 -threads {Environment.ProcessorCount} -speed 1";

      // Pass 1 cache
      string logPath = Path.Combine(Environment.CurrentDirectory, $"temp_{code}-0.log");
      Cache.FPCKey key = new Cache.FPCKey(file, vf.ToString(), startString, timeLengthString);
      bool cached = Cache.Instance.CreateIfPossible(key, logPath);

      // If CRF_MODE
      if (crf != ushort.MaxValue)
      {
        if (!cached)
        {
          args = $"-hide_banner -y -ss {startString} -i \"{file}\" -c:v vp9 -pix_fmt +yuv420p {vf} -crf {crf} -b:v 0 {threadSettings} -an -t {timeLengthString} -sn -lag-in-frames 25 -pass 1 -auto-alt-ref 1 -passlogfile temp_{code} -f null -y NUL";
          ExecuteFFMPEG(args, pu);
          Cache.Instance.Save(key, logPath);
        }

        args = $"-hide_banner -y -ss {startString} -i \"{file}\" -c:v vp9 -pix_fmt +yuv420p {vf} -crf {crf} -b:v 0 {threadSettings} -an -t {timeLengthString} -sn -lag-in-frames 25 -pass 2 -auto-alt-ref 1 -passlogfile temp_{code} \"{webmPath}\"";
        ExecuteFFMPEG(args, pu);
      }
      else
      {
        double audioSize = GetSizeKB(oggPath);
        int bitrate = (int)((sizeLimit - audioSize) * 8 / timeLength.TotalSeconds);
        string bitrateString = $"-b:v {bitrate}K";

        if (!cached)
        {
          args = $"-hide_banner -y -ss {startString} -i \"{file}\" -c:v vp9 -pix_fmt +yuv420p {bitrateString} {threadSettings} -an {vf} -t {timeLengthString} -sn {otherVideo} -lag-in-frames 25 -pass 1 -auto-alt-ref 1 -passlogfile temp_{code} -f null -y NUL";
          ExecuteFFMPEG(args, pu);
          Cache.Instance.Save(key, logPath);
        }

        args = $"-hide_banner -y -ss {startString} -i \"{file}\" -c:v vp9 -pix_fmt +yuv420p {bitrateString} {threadSettings} -an {vf} -t {timeLengthString} -sn {otherVideo} -lag-in-frames 25 -pass 2 -auto-alt-ref 1 -passlogfile temp_{code} \"{webmPath}\"";
        ExecuteFFMPEG(args, pu);
      }

      // Concat
      args = $"-hide_banner -y -i \"{webmPath}\" -i \"{oggPath}\" -c copy -metadata title=\"{Path.GetFileNameWithoutExtension(file)} [github.com/CherryPerry/ffmpeg-vp9-wrap]\" \"{finalPath}\"";
      ExecuteFFMPEG(args, pu);

      // Delete
      if (subsWereCopied)
        File.Delete(subs);
      File.Delete(webmPath);
      File.Delete(oggPath);
      File.Delete(logPath);

      sp.Destroy(pu);

      return finalPath;
    }

    static string GetCrop(string file, string start, string t)
    {
      string args = $"-hide_banner -ss {start} -i \"{file}\" -t {t} -vf cropdetect=64:2:0 -f null NUL";
      string cached = Cache.Instance.Get<string>(Cache.CACHE_STRINGS, args);
      if (cached == null)
      {
        string output = new Executer(Executer.FFMPEG).Execute(args);
        Regex regex = new Regex(@".*(crop=\d+:\d+:\d+:\d+).*");
        Match match = regex.Match(output);
        if (!match.Success)
          return null;
        cached = match.Groups[match.Groups.Count - 1].Value;
        Cache.Instance.Put(Cache.CACHE_STRINGS, args, cached);
      }
      return cached;
    }

    static string DownloadVideo()
    {
      string link = ArgList.Get(Arg.YOUTUBE).AsString();
      IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);
      VideoInfo vi = videoInfos.OrderByDescending(x => x.Resolution).First(x => x.VideoType == VideoType.Mp4 && x.AudioExtension != null);
      if (vi.RequiresDecryption)
        DownloadUrlResolver.DecryptDownloadUrl(vi);
      VideoDownloader vd = new VideoDownloader(vi, Path.Combine(Environment.CurrentDirectory, vi.Title + vi.VideoExtension));
      using (Mutex mutex = new Mutex())
      {
        vd.DownloadFinished += (sender, e) => { try { mutex.ReleaseMutex(); } catch { } };
        vd.DownloadProgressChanged += (sender, e) => { Console.WriteLine("Downloading {0}%", e.ProgressPercentage); };
        vd.Execute();
        mutex.WaitOne();
      }
      return vd.SavePath;
    }

    static void LookupEndTime(string filePath)
    {
      if (!ArgList.Get(Arg.END_TIME))
      {
        string value = Ffprober.Probe(filePath).EndTime;
        if (value != null)
          ArgList.Get(Arg.END_TIME).Value = value;
      }
    }

    // method 1: add preview as separate track with slightly bigger resolution (via Pituz)
    static void GeneratePreview(string filePath)
    {
      OutputProcessor sp = new SimpleProcessor();
      ProcessingUnit pu = sp.CreateOne();

      string fileName = Path.GetFileName(filePath),
             output = filePath.Substring(0, filePath.LastIndexOf('.') + 1) + "preview.webm",
             previewSource = ArgList.Get(Arg.PREVIEW_SOURCE) ? GetFullPath(ArgList.Get(Arg.PREVIEW_SOURCE).AsString()) : filePath,
             previewTiming = ArgList.Get(Arg.PREVIEW).AsString();

      // Preview
      long time = DateTime.Now.ToFileTimeUtc();
      string previewWebm = GetFolder(filePath) + "\\preview_" + time.ToString() + ".webm";

      // Same scale
      Ffprober.Result result = Ffprober.Probe(filePath);
      string scale = result.Scale;
      scale = scale == null ? string.Empty : $",scale={result.WidthPix + 1}:-1";
      scale = $"-filter:v:1 \"trim=end_frame=2{scale}\"";
      string args = $"-hide_banner -i \"{filePath}\" -ss {previewTiming} -i \"{previewSource}\" -c copy -map 0:v -map 0:a -map 1:v -c:v:1 vp9 -b:v:1 0 -crf 8 -speed 1 {scale} \"{output}\"";
      ExecuteFFMPEG(args, pu);

      sp.Destroy(pu);
    }

    [Obsolete]
    static void GeneratePreviewOld(string filePath)
    {
      OutputProcessor sp = new SimpleProcessor();
      ProcessingUnit pu = sp.CreateOne();

      string fileName = Path.GetFileName(filePath),
             output = filePath.Substring(0, filePath.LastIndexOf('.') + 1) + "preview.webm",
             previewSource = ArgList.Get(Arg.PREVIEW_SOURCE) ? GetFullPath(ArgList.Get(Arg.PREVIEW_SOURCE).AsString()) : filePath,
             previewTiming = ArgList.Get(Arg.PREVIEW).AsString();

      // Preview
      long time = DateTime.Now.ToFileTimeUtc();
      string previewWebm = GetFolder(filePath) + "\\preview_" + time.ToString() + ".webm";

      // Same scale
      Ffprober.Result result = Ffprober.Probe(filePath);
      string scale = result.Scale;
      scale = scale == null ? string.Empty : (",scale=" + scale);
      scale = $"-vf \"trim=end_frame=2{scale}\"";
      string args = $"-hide_banner -ss {previewTiming} -i \"{previewSource}\" -c:v vp9 -pix_fmt +yuv420p -b:v 0 -crf 8 -speed 1 -an -sn {scale} \"{previewWebm}\"";
      ExecuteFFMPEG(args, pu);

      // Bad muxing fix
      string videoOnly = $"video_{time}.webm";
      args = $"-hide_banner -i \"{filePath}\" -c copy -map 0:v \"{videoOnly}\"";
      ExecuteFFMPEG(args, pu);

      // Concat
      string concatedWebm = $"concat_{time}.webm";
      string concatFile = $"concat_{time}.txt";
      File.WriteAllText(concatFile, $"file '{previewWebm}'\r\nfile '{videoOnly}'", Encoding.Default);
      string fps = result.Framerate;
      string dur = Ffprober.Probe(previewWebm).EndTime;
      if (dur == null)
        dur = "0.042";
      args = $"-hide_banner -f concat -i \"{concatFile}\" -c copy \"{concatedWebm}\"";
      ExecuteFFMPEG(args, pu);
      args = $"-hide_banner -y -itsoffset 0.5 -i \"{filePath}\" -i \"{concatedWebm}\" -map 1:v -map 0:a -c copy \"{output}\"";
      ExecuteFFMPEG(args, pu);

      // Delete
      File.Delete(concatFile);
      File.Delete(previewWebm);
      File.Delete(concatedWebm);
      File.Delete(videoOnly);

      sp.Destroy(pu);
    }

    static void ExecuteFFMPEG(string args, ProcessingUnit pu)
    {
      Process proc = new Process();
      proc.StartInfo.FileName = "ffmpeg.exe";
      proc.StartInfo.Arguments = args;
      proc.StartInfo.UseShellExecute = false;
      proc.StartInfo.RedirectStandardOutput = true;
      proc.StartInfo.RedirectStandardError = true;
      proc.ErrorDataReceived += pu.DataReceived;
      proc.OutputDataReceived += pu.DataReceived;
      pu.Write("\n\n" + args + "\n\n");
      proc.Start();
      proc.PriorityClass = ProcessPriorityClass.Idle;
      proc.BeginOutputReadLine();
      proc.BeginErrorReadLine();
      proc.WaitForExit();
      proc.Close();
    }

    static void DataReceived(object sender, DataReceivedEventArgs data)
    {
      if (data.Data != null && data.Data.Length == Console.WindowWidth)
        Console.Write(data.Data);
      else
        Console.WriteLine(data.Data);
    }

    static double GetSize(string path)
    {
      return new FileInfo(path).Length;
    }

    static double GetSizeKB(string path)
    {
      return GetSize(path) / 1024d;
    }

    static double GetSizeMB(string path)
    {
      return GetSizeKB(path) / 1024d;
    }

    static string GetFullPath(string file)
    {
      return Path.Combine(GetFolder(file), Path.GetFileName(file));
    }

    static string GetFolder(string file)
    {
      string pathToFile = Path.GetDirectoryName(file);
      if (string.IsNullOrEmpty(pathToFile))
        pathToFile = Environment.CurrentDirectory;
      return pathToFile;
    }

    internal static TimeSpan ParseToTimespan(string str)
    {
      try { return TimeSpan.ParseExact(str, "hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture); }
      catch (FormatException)
      {
        try { return TimeSpan.ParseExact(str, "mm\\:ss\\.fff", CultureInfo.InvariantCulture); }
        catch (FormatException) { return TimeSpan.FromSeconds(Double.Parse(str, new CultureInfo("en"))); }
      }
    }
  }
}
