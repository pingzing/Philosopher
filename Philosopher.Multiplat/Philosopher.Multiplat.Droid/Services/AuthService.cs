using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Runtime;
using Philosopher.Multiplat.Services;
using Java.Security;
using Java.Util;
using Javax.Crypto;
using Java.IO;
using Philosopher.Multiplat.Droid.Services;

[assembly: Xamarin.Forms.Dependency(typeof(AuthService))]
namespace Philosopher.Multiplat.Droid.Services
{
    public class AuthService : IAuthService
    {
        private Context _context;
        private KeyStore _keyStore;
        private KeyStore.PasswordProtection _prot;

        static readonly object fileLock = new object();

        private const string _fileName = "Philosopher.Accounts";
        private static readonly char[] _password = "3295043EA18CA264B2C40E0B72051DEF2D07AD2B4593F43DDDE1515A7EC32617".ToCharArray(); //todo: change this

        private void InitializeStore()
        {
            this._context = Android.App.Application.Context;
            _keyStore = KeyStore.GetInstance(KeyStore.DefaultType);
            _prot = new KeyStore.PasswordProtection(_password);

            try
            {
                lock (fileLock)
                {
                   if(!this.FileExists(_context, _fileName))
                    {
                        LoadEmptyKeyStore(_password);
                    }
                   else
                    {
                        using (var f = _context.OpenFileInput(_fileName))
                        {
                            _keyStore.Load(f, _password);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is Java.IO.FileNotFoundException || ex is System.IO.FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine($"Caught {ex.GetType().ToString()} in Android.AuthService.InitializeStore.");
                LoadEmptyKeyStore(_password);
            }
        }

        public void Delete(Account account, string serviceId)
        {
            if(_keyStore == null)
            {
                InitializeStore();
            }

            string alias = $"{account.Username}-{serviceId}";
            _keyStore.DeleteEntry(alias);
            SaveCurrent();
        }

        public IEnumerable<Account> FindAccountsForService(string serviceId)
        {
            if (_keyStore == null)
            {
                InitializeStore();
            }
            
            var accounts = new List<Account>();
            string postfix = "-" + serviceId;
            IEnumeration aliases = _keyStore.Aliases();
            while(aliases.HasMoreElements)
            {
                string alias = aliases.NextElement().ToString();
                if(alias.EndsWith(postfix))
                {
                    var entry = _keyStore.GetEntry(alias, _prot) as KeyStore.SecretKeyEntry;
                    if(entry != null)
                    {
                        byte[] bytes = entry.SecretKey.GetEncoded();
                        string serialized = Encoding.UTF8.GetString(bytes);
                        Account account = Account.Deserialize(serialized);
                        accounts.Add(account);
                    }
                }
            }
            return accounts;
        }

        public void Save(Account account, string serviceId)
        {
            if (_keyStore == null)
            {
                InitializeStore();
            }

            string alias = $"{account.Username}-{serviceId}";

            var secretKey = new SecretAccount(account);
            var entry = new KeyStore.SecretKeyEntry(secretKey);
            _keyStore.SetEntry(alias, entry, _prot);
            SaveCurrent();
        }

        private void SaveCurrent()
        {
            lock(fileLock)
            {
                using (var s = _context.OpenFileOutput(_fileName, FileCreationMode.Private))
                {
                    _keyStore.Store(s, _password);
                }
            }
        }

        public bool FileExists(Context context, String filename)
        {
            File file = context.GetFileStreamPath(filename);
            if (file == null || !file.Exists())
            {
                return false;
            }

            return true;
        }


        private static IntPtr id_load_Ljava_io_InputStream_arrayC;

        private void LoadEmptyKeyStore(char[] password)
        {
            if (id_load_Ljava_io_InputStream_arrayC == IntPtr.Zero)
            {
                id_load_Ljava_io_InputStream_arrayC = JNIEnv.GetMethodID(_keyStore.Class.Handle, "load", "(Ljava/io/InputStream;[C)V");
            }
            IntPtr intPtr = IntPtr.Zero;
            IntPtr intPtr2 = JNIEnv.NewArray(password);
            JNIEnv.CallVoidMethod(_keyStore.Handle, id_load_Ljava_io_InputStream_arrayC, new JValue[]
                {
                new JValue (intPtr),
                new JValue (intPtr2)
                });
            JNIEnv.DeleteLocalRef(intPtr);
            if (password != null)
            {
                JNIEnv.CopyArray(intPtr2, password);
                JNIEnv.DeleteLocalRef(intPtr2);
            }
        }
    }



    class SecretAccount : Java.Lang.Object, ISecretKey
    {
        byte[] bytes;
        public SecretAccount(Account account)
        {
            bytes = Encoding.UTF8.GetBytes(account.Serialize());
        }
        public byte[] GetEncoded()
        {
            return bytes;
        }
        public string Algorithm
        {
            get
            {
                return "RAW";
            }
        }
        public string Format
        {
            get
            {
                return "RAW";
            }
        }
    }
}