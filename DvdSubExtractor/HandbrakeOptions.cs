using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DvdSubExtractor
{
    class EncoderOptions
    {
        public EncoderOptions()
        {
        }

        public EncoderOptions(EncoderOptions other)
        {
            this.Encoder = other.Encoder;
            this.Mixing = other.Mixing;
            this.Bitrate = other.Bitrate;
        }

        public AudioEncoder Encoder { get; set; }
        public AudioMixing Mixing { get; set; }
        public AudioBitrate Bitrate { get; set; }
    }

    class VideoProfile
    {
        public VideoProfile(string name)
        {
            this.Name = name;
            this.Detelecine = VideoDetelecine.On;
            this.Deinterlace = VideoDeinterlace.Decomb;
            this.Deblock = VideoDeblock.Off;
            this.Denoise = VideoDenoise.Off;
            this.QualityType = VideoQualityType.ConstantQuality;
            this.Quality = 17.0;
            this.Bitrate = 1000.0;
            this.x264Options = "level=4.1:ref=4:mixed-refs=1:b-adapt=2";
            this.ExtraOptions = "";
        }

        public VideoProfile(VideoProfile other)
        {
            this.Name = other.Name;
            this.Detelecine = other.Detelecine;
            this.Deinterlace = other.Deinterlace;
            this.Deblock = other.Deblock;
            this.Denoise = other.Denoise;
            this.QualityType = other.QualityType;
            this.Quality = other.Quality;
            this.Bitrate = other.Bitrate;
            this.x264Options = other.x264Options;
            this.ExtraOptions = other.ExtraOptions;
        }

        public string Name { get; set; }
        public VideoDetelecine Detelecine { get; set; }
        public VideoDeinterlace Deinterlace { get; set; }
        public VideoDenoise Denoise { get; set; }
        public VideoDeblock Deblock { get; set; }
        public VideoQualityType QualityType { get; set; }
        public double Quality { get; set; }
        public double Bitrate { get; set; }
        public string x264Options { get; set; }
        public string ExtraOptions { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

        public const double ConstantQualityMax = 50.0;
        public const double ConstantQualityMin = 5.0;
        public const double ConstantQualityStep = .25;

        public const int AverageBitrateMax = 50000;
        public const int AverageBitrateMin = 500;
        public const int AverageBitrateStep = 250;
    }

    class HandbrakeOptions
    {
        public EncoderOptions Ac3Channels6 { get; set; }
        public EncoderOptions Ac3Channels2 { get; set; }
        public EncoderOptions DtsChannels6 { get; set; }
        public EncoderOptions Mpeg2Channels2 { get; set; }
        public EncoderOptions LpcmChannels2 { get; set; }

        public IList<VideoProfile> Profiles { get; private set; }

        public HandbrakeOptions()
        {
            this.Profiles = new List<VideoProfile>();
        }

        public HandbrakeOptions(HandbrakeOptions other)
        {
            this.Ac3Channels6 = other.Ac3Channels6;
            this.Ac3Channels2 = other.Ac3Channels2;
            this.DtsChannels6 = other.DtsChannels6;
            this.Mpeg2Channels2 = other.Mpeg2Channels2;
            this.LpcmChannels2 = other.LpcmChannels2;

            foreach(VideoProfile profile in other.Profiles)
            {
                this.Profiles.Add(new VideoProfile(profile));
            }
        }
    }

    static class HandbrakeXml
    {
        public static HandbrakeOptions Load(string xml)
        {
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(xml);
                return Load(xdoc.DocumentElement);
            }
            catch(Exception)
            {
                return null;
            }
        }

        static T ParseEnum<T>(XmlElement element, string attribute) where T : struct
        {
            if(element.HasAttribute(attribute))
            {
                string value = element.GetAttribute(attribute);
                try
                {
                    return (T)Enum.Parse(typeof(T), value);
                }
                catch(Exception)
                {
                }
            }
            return default(T);
        }

        static EncoderOptions LoadEncoder(XmlElement element)
        {
            if(element != null)
            {
                try
                {
                    return new EncoderOptions()
                    {
                        Encoder = ParseEnum<AudioEncoder>(element, "Encoder"),
                        Mixing = ParseEnum<AudioMixing>(element, "Mixing"),
                        Bitrate = ParseEnum<AudioBitrate>(element, "Bitrate")
                    };
                }
                catch(Exception)
                {
                }
            }
            return new EncoderOptions();
        }

        static VideoProfile LoadProfile(XmlElement element)
        {
            if((element == null) || !element.HasAttribute("Name"))
            {
                return new VideoProfile("Load Failure");
            }

            VideoProfile profile = new VideoProfile(element.GetAttribute("Name"));
            profile.Detelecine = ParseEnum<VideoDetelecine>(element, "Detelecine");
            profile.Deinterlace = ParseEnum<VideoDeinterlace>(element, "Deinterlace");
            profile.Denoise = ParseEnum<VideoDenoise>(element, "Denoise");
            profile.Deblock = ParseEnum<VideoDeblock>(element, "Deblock");
            profile.QualityType = ParseEnum<VideoQualityType>(element, "QualityType");
            double quality;
            if(double.TryParse(element.GetAttribute("Quality"), out quality))
            {
                profile.Quality = quality;
            }
            if(double.TryParse(element.GetAttribute("Bitrate"), out quality))
            {
                profile.Bitrate = quality;
            }
            profile.x264Options = element.GetAttribute("x264Options");
            profile.ExtraOptions = element.GetAttribute("ExtraOptions");
            return profile;
        }

        public static HandbrakeOptions Load(XmlNode rootElement)
        {
            HandbrakeOptions options = new HandbrakeOptions();

            options.Ac3Channels6 = LoadEncoder(
                rootElement.SelectSingleNode("Encoder[@Name='Ac3Channels6']") as XmlElement);
            options.Ac3Channels2 = LoadEncoder(
                rootElement.SelectSingleNode("Encoder[@Name='Ac3Channels2']") as XmlElement);
            options.DtsChannels6 = LoadEncoder(
                rootElement.SelectSingleNode("Encoder[@Name='DtsChannels6']") as XmlElement);
            options.Mpeg2Channels2 = LoadEncoder(
                rootElement.SelectSingleNode("Encoder[@Name='Mpeg2Channels2']") as XmlElement);
            options.LpcmChannels2 = LoadEncoder(
                rootElement.SelectSingleNode("Encoder[@Name='LpcmChannels2']") as XmlElement);

            foreach(XmlElement profileElement in rootElement.SelectNodes("Profile"))
            {
                options.Profiles.Add(LoadProfile(profileElement));
            }
            return options;
        }

        public static string Save(HandbrakeOptions options)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlElement rootElem = xdoc.CreateElement("Handbrake");
            xdoc.AppendChild(rootElem);
            Save(options, rootElem);

            StringWriter writer = new StringWriter();
            xdoc.Save(writer);
            return writer.ToString();
        }

        static void SaveEncoder(string name, EncoderOptions encOptions, XmlElement rootElement)
        {
            XmlElement element = rootElement.OwnerDocument.CreateElement("Encoder");
            rootElement.AppendChild(element);

            element.SetAttribute("Name", name);
            element.SetAttribute("Encoder", encOptions.Encoder.ToString());
            element.SetAttribute("Mixing", encOptions.Mixing.ToString());
            element.SetAttribute("Bitrate", encOptions.Bitrate.ToString());
        }

        static void SaveProfile(VideoProfile profile, XmlElement rootElement)
        {
            XmlElement element = rootElement.OwnerDocument.CreateElement("Profile");
            rootElement.AppendChild(element);

            element.SetAttribute("Name", profile.Name);
            element.SetAttribute("Detelecine", profile.Detelecine.ToString());
            element.SetAttribute("Deinterlace", profile.Deinterlace.ToString());
            element.SetAttribute("Denoise", profile.Denoise.ToString());
            element.SetAttribute("Deblock", profile.Deblock.ToString());
            element.SetAttribute("QualityType", profile.QualityType.ToString());
            element.SetAttribute("Quality", profile.Quality.ToString("f2"));
            element.SetAttribute("Bitrate", profile.Bitrate.ToString("f0"));
            element.SetAttribute("x264Options", profile.x264Options);
            element.SetAttribute("ExtraOptions", profile.ExtraOptions);
        }

        public static void Save(HandbrakeOptions options, XmlElement rootElement)
        {
            SaveEncoder("Ac3Channels6", options.Ac3Channels6, rootElement);
            SaveEncoder("Ac3Channels2", options.Ac3Channels2, rootElement);
            SaveEncoder("DtsChannels6", options.DtsChannels6, rootElement);
            SaveEncoder("Mpeg2Channels2", options.Mpeg2Channels2, rootElement);
            SaveEncoder("LpcmChannels2", options.LpcmChannels2, rootElement);

            foreach(VideoProfile profile in options.Profiles)
            {
                SaveProfile(profile, rootElement);
            }
        }
    }
}
