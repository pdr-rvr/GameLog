using System;
using System.Security.Cryptography;

namespace GameLog_Backend.Helpers
{
    /// <summary>
    /// Gerador de identificadores únicos UUIDv7 (RFC 9562) ordenáveis temporalmente.
    /// Combina timestamp Unix de 48 bits em milissegundos com entropia criptográfica.
    /// </summary>
    public static class UuidV7Helper
    {
        public static Guid NewGuid()
        {
            Span<byte> bytes = stackalloc byte[16];

            // 48-bit timestamp em milissegundos (Big-Endian)
            long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            bytes[0] = (byte)(unixMs >> 40);
            bytes[1] = (byte)(unixMs >> 32);
            bytes[2] = (byte)(unixMs >> 24);
            bytes[3] = (byte)(unixMs >> 16);
            bytes[4] = (byte)(unixMs >> 8);
            bytes[5] = (byte)unixMs;

            // Preenche os 10 bytes restantes com entropia criptográfica
            RandomNumberGenerator.Fill(bytes.Slice(6, 10));

            // Ajusta o nibble de versão para 7 (0111) no byte 6
            bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70);

            // Ajusta os 2 bits da variante para RFC 9562 (10xx) no byte 8
            bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

            // Swap para formato interno do .NET Guid em arquiteturas Little-Endian
            if (BitConverter.IsLittleEndian)
            {
                // Swap _a (bytes 0-3)
                byte temp = bytes[0]; bytes[0] = bytes[3]; bytes[3] = temp;
                temp = bytes[1]; bytes[1] = bytes[2]; bytes[2] = temp;

                // Swap _b (bytes 4-5)
                temp = bytes[4]; bytes[4] = bytes[5]; bytes[5] = temp;

                // Swap _c (bytes 6-7)
                temp = bytes[6]; bytes[6] = bytes[7]; bytes[7] = temp;
            }

            return new Guid(bytes);
        }
    }
}
