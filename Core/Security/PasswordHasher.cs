using System;
using System.Security.Cryptography;
namespace HelpFastDesktop.Core.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100_000;
    private const byte Version = 0x01;

    public static string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Senha não pode ser vazia.", nameof(password));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        var payload = new byte[1 + sizeof(int) + SaltSize + HashSize];
        payload[0] = Version;
        BitConverter.GetBytes(Iterations).CopyTo(payload, 1);
        Buffer.BlockCopy(salt, 0, payload, 1 + sizeof(int), SaltSize);
        Buffer.BlockCopy(hash, 0, payload, 1 + sizeof(int) + SaltSize, HashSize);

        return Convert.ToBase64String(payload);
    }

    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        if (TryDecodePayload(storedHash, out var iterations, out var salt, out var hash))
        {
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hash.Length);
            return CryptographicOperations.FixedTimeEquals(hash, computedHash);
        }

        if (TryDecodeLegacyIdentityPayload(storedHash, out var legacyParameters))
        {
            var (prf, iterationsLegacy, saltLegacy, hashLegacy) = legacyParameters;
            var algorithm = prf switch
            {
                0 => HashAlgorithmName.SHA1,
                1 => HashAlgorithmName.SHA256,
                2 => HashAlgorithmName.SHA512,
                _ => HashAlgorithmName.SHA256
            };

            var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, saltLegacy, iterationsLegacy, algorithm, hashLegacy.Length);
            return CryptographicOperations.FixedTimeEquals(hashLegacy, computedHash);
        }

        return false;
    }

    public static bool IsHashed(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return TryDecodePayload(value, out _, out _, out _) ||
               TryDecodeLegacyIdentityPayload(value, out _);
    }

    public static bool IsCurrentFormat(string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               TryDecodePayload(value, out _, out _, out _);
    }

    private static bool TryDecodePayload(string storedHash, out int iterations, out byte[] salt, out byte[] hash)
    {
        iterations = default;
        salt = Array.Empty<byte>();
        hash = Array.Empty<byte>();

        try
        {
            var payload = Convert.FromBase64String(storedHash);

            if (payload.Length != 1 + sizeof(int) + SaltSize + HashSize)
            {
                return false;
            }

            if (payload[0] != Version)
            {
                return false;
            }

            iterations = BitConverter.ToInt32(payload, 1);
            salt = new byte[SaltSize];
            hash = new byte[HashSize];

            Buffer.BlockCopy(payload, 1 + sizeof(int), salt, 0, SaltSize);
            Buffer.BlockCopy(payload, 1 + sizeof(int) + SaltSize, hash, 0, HashSize);

            return iterations > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TryDecodeLegacyIdentityPayload(string storedHash, out (int prf, int iterations, byte[] salt, byte[] hash) parameters)
    {
        parameters = default;

        try
        {
            var payload = Convert.FromBase64String(storedHash);

            if (payload.Length < 1 + sizeof(int) * 3)
            {
                return false;
            }

            if (payload[0] != Version)
            {
                return false;
            }

            var prf = ReadNetworkByteOrder(payload, 1);
            var iterations = ReadNetworkByteOrder(payload, 5);
            var saltLength = ReadNetworkByteOrder(payload, 9);

            if (saltLength <= 0 || saltLength > payload.Length - 13)
            {
                return false;
            }

            var salt = new byte[saltLength];
            Buffer.BlockCopy(payload, 13, salt, 0, saltLength);

            var subKeyOffset = 13 + saltLength;
            if (payload.Length < subKeyOffset + sizeof(int))
            {
                return false;
            }

            var subKeyLength = ReadNetworkByteOrder(payload, subKeyOffset);
            if (subKeyLength <= 0 || payload.Length < subKeyOffset + sizeof(int) + subKeyLength)
            {
                return false;
            }

            var subKey = new byte[subKeyLength];
            Buffer.BlockCopy(payload, subKeyOffset + sizeof(int), subKey, 0, subKeyLength);

            parameters = (prf, iterations, salt, subKey);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static int ReadNetworkByteOrder(byte[] buffer, int offset)
    {
        return (buffer[offset] << 24)
             | (buffer[offset + 1] << 16)
             | (buffer[offset + 2] << 8)
             | buffer[offset + 3];
    }
}

