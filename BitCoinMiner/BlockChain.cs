namespace BitCoinMiner
{
    using System;
    using System.Collections.Generic;

    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Globalization;
    public partial class AppSettings
    {
        [JsonPropertyName("Miners")]
        public List<Miner> Miners { get; set; }
    }

    public partial class Miner
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("WalletAddress")]
        public List<string> WalletAddress { get; set; }
    }
    public partial class BlockChain
    {
        [JsonPropertyName("hash160")]
        public string Hash160 { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("n_tx")]
        public long NTx { get; set; }

        [JsonPropertyName("n_unredeemed")]
        public long NUnredeemed { get; set; }

        [JsonPropertyName("total_received")]
        public long TotalReceived { get; set; }

        [JsonPropertyName("total_sent")]
        public long TotalSent { get; set; }

        [JsonPropertyName("final_balance")]
        public long FinalBalance { get; set; }

        [JsonPropertyName("txs")]
        public List<Tx> Txs { get; set; }
    }

    public partial class Tx
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("ver")]
        public long Ver { get; set; }

        [JsonPropertyName("vin_sz")]
        public long VinSz { get; set; }

        [JsonPropertyName("vout_sz")]
        public long VoutSz { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("weight")]
        public long Weight { get; set; }

        [JsonPropertyName("fee")]
        public long Fee { get; set; }

        [JsonPropertyName("relayed_by")]
        public RelayedBy RelayedBy { get; set; }

        [JsonPropertyName("lock_time")]
        public long LockTime { get; set; }

        [JsonPropertyName("tx_index")]
        public long TxIndex { get; set; }

        [JsonPropertyName("double_spend")]
        public bool DoubleSpend { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("block_index")]
        public long BlockIndex { get; set; }

        [JsonPropertyName("block_height")]
        public long BlockHeight { get; set; }

        [JsonPropertyName("inputs")]
        public List<Input> Inputs { get; set; }

        [JsonPropertyName("out")]
        public List<Out> Out { get; set; }

        [JsonPropertyName("result")]
        public long Result { get; set; }

        [JsonPropertyName("balance")]
        public long Balance { get; set; }
    }

    public partial class Input
    {
        [JsonPropertyName("sequence")]
        public long Sequence { get; set; }

        [JsonPropertyName("witness")]
        public string Witness { get; set; }

        [JsonPropertyName("script")]
        public string Script { get; set; }

        [JsonPropertyName("index")]
        public long Index { get; set; }

        [JsonPropertyName("prev_out")]
        public Out PrevOut { get; set; }
    }

    public partial class Out
    {
        [JsonPropertyName("addr")]
        public string Addr { get; set; }

        [JsonPropertyName("n")]
        public long N { get; set; }

        [JsonPropertyName("script")]
        public string Script { get; set; }

        [JsonPropertyName("spending_outpoints")]
        public List<SpendingOutpoint> SpendingOutpoints { get; set; }

        [JsonPropertyName("spent")]
        public bool Spent { get; set; }

        [JsonPropertyName("tx_index")]
        public long TxIndex { get; set; }

        [JsonPropertyName("type")]
        public long Type { get; set; }

        [JsonPropertyName("value")]
        public long Value { get; set; }
    }

    public partial class SpendingOutpoint
    {
        [JsonPropertyName("n")]
        public long N { get; set; }

        [JsonPropertyName("tx_index")]
        public long TxIndex { get; set; }
    }

    public enum RelayedBy { The0000 };

    internal static class Converter
    {
        public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General)
        {
            Converters =
            {
                RelayedByConverter.Singleton,
                new DateOnlyConverter(),
                new TimeOnlyConverter(),
                IsoDateTimeOffsetConverter.Singleton
            },
        };
    }

    internal class RelayedByConverter : JsonConverter<RelayedBy>
    {
        public override bool CanConvert(Type t) => t == typeof(RelayedBy);

        public override RelayedBy Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == "0.0.0.0")
            {
                return RelayedBy.The0000;
            }
            throw new Exception("Cannot unmarshal type RelayedBy");
        }

        public override void Write(Utf8JsonWriter writer, RelayedBy value, JsonSerializerOptions options)
        {
            if (value == RelayedBy.The0000)
            {
                JsonSerializer.Serialize(writer, "0.0.0.0", options);
                return;
            }
            throw new Exception("Cannot marshal type RelayedBy");
        }

        public static readonly RelayedByConverter Singleton = new RelayedByConverter();
    }

    public class DateOnlyConverter : JsonConverter<DateOnly>
    {
        private readonly string serializationFormat;
        public DateOnlyConverter() : this(null) { }

        public DateOnlyConverter(string? serializationFormat)
        {
            this.serializationFormat = serializationFormat ?? "yyyy-MM-dd";
        }

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return DateOnly.Parse(value!);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(serializationFormat));
    }

    public class TimeOnlyConverter : JsonConverter<TimeOnly>
    {
        private readonly string serializationFormat;

        public TimeOnlyConverter() : this(null) { }

        public TimeOnlyConverter(string? serializationFormat)
        {
            this.serializationFormat = serializationFormat ?? "HH:mm:ss.fff";
        }

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return TimeOnly.Parse(value!);
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(serializationFormat));
    }

    internal class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override bool CanConvert(Type t) => t == typeof(DateTimeOffset);

        private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

        private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
        private string? _dateTimeFormat;
        private CultureInfo? _culture;

        public DateTimeStyles DateTimeStyles
        {
            get => _dateTimeStyles;
            set => _dateTimeStyles = value;
        }

        public string? DateTimeFormat
        {
            get => _dateTimeFormat ?? string.Empty;
            set => _dateTimeFormat = string.IsNullOrEmpty(value) ? null : value;
        }

        public CultureInfo Culture
        {
            get => _culture ?? CultureInfo.CurrentCulture;
            set => _culture = value;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            string text;


            if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
                || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
            {
                value = value.ToUniversalTime();
            }

            text = value.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);

            writer.WriteStringValue(text);
        }

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? dateText = reader.GetString();

            if (string.IsNullOrEmpty(dateText) == false)
            {
                if (!string.IsNullOrEmpty(_dateTimeFormat))
                {
                    return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
                }
                else
                {
                    return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
                }
            }
            else
            {
                return default;
            }
        }


        public static readonly IsoDateTimeOffsetConverter Singleton = new IsoDateTimeOffsetConverter();
    }
}
