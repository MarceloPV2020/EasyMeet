using System.Runtime.InteropServices;
using System.Text;
using EasyMeet.Api.Models;

namespace EasyMeet.Api.Services;

public sealed class WindowsCredentialApiKeyStore : IApiKeyStore
{
    private const uint CredTypeGeneric = 1;
    private const uint CredPersistLocalMachine = 2;

    public Task SaveApiKeyAsync(ProvedorIA provedor, string apiKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var target = BuildTarget(provedor);
        var bytes = Encoding.Unicode.GetBytes(apiKey);
        var credential = new NativeCredential
        {
            Type = CredTypeGeneric,
            TargetName = target,
            CredentialBlobSize = (uint)bytes.Length,
            Persist = CredPersistLocalMachine,
            UserName = "EasyMeet"
        };

        var blobPtr = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, blobPtr, bytes.Length);
            credential.CredentialBlob = blobPtr;
            if (!CredWrite(ref credential, 0))
            {
                throw new InvalidOperationException("Falha ao salvar chave no Windows Credential Manager.");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(blobPtr);
        }

        return Task.CompletedTask;
    }

    public Task<string?> GetApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var target = BuildTarget(provedor);

        if (!CredRead(target, CredTypeGeneric, 0, out var credPtr) || credPtr == IntPtr.Zero)
        {
            return Task.FromResult<string?>(null);
        }

        try
        {
            var cred = Marshal.PtrToStructure<NativeCredential>(credPtr);
            if (cred.CredentialBlob == IntPtr.Zero || cred.CredentialBlobSize == 0)
            {
                return Task.FromResult<string?>(null);
            }

            var value = Marshal.PtrToStringUni(cred.CredentialBlob, (int)cred.CredentialBlobSize / 2);
            return Task.FromResult<string?>(value);
        }
        finally
        {
            CredFree(credPtr);
        }
    }

    public async Task DeleteApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var target = BuildTarget(provedor);
        _ = CredDelete(target, CredTypeGeneric, 0);
        await Task.CompletedTask;
    }

    public async Task<bool> HasApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken)
    {
        var value = await GetApiKeyAsync(provedor, cancellationToken);
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string BuildTarget(ProvedorIA provedorIA) => $"EasyMeet:{provedorIA}";

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeCredential
    {
        public uint Flags;
        public uint Type;
        public string TargetName;
        public string Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias;
        public string UserName;
    }

    [DllImport("advapi32", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite([In] ref NativeCredential userCredential, [In] uint flags);

    [DllImport("advapi32", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, uint type, uint reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(string target, uint type, uint flags);

    [DllImport("advapi32", SetLastError = true)]
    private static extern void CredFree([In] IntPtr cred);
}
