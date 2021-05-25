using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileSystem
{
    class FileSystem
    {

        public static readonly int duzinaFajla = 64 * 1024; // max velicina fajla
        public static readonly int velicinaFS = 20 * 1024 * 1024; // max velicina file sistema
        public static readonly int velicinaBloka = 256; // max velicina bloka za cuvanje sadrzaja
        public static int brDatoteka = 0; // ukupan broj fajlova na fs
        public static int brDirektorijuma = 0; // ukupan br direktorijuma na fs
        public static int iskoriscenProstor = 76;
        public static int slobodanProstor = duzinaFajla - iskoriscenProstor;
        public static int glavniSlobodanProstor = velicinaFS - glavniIskoriscenProstor;
        public static int glavniIskoriscenProstor = 0;
        public int initialSize = 0;
        public static readonly string separator = "//////////////////////////////////////////";
        public void ExecuteFileSystem()
        {
            Console.WriteLine("pravim fajl sistem.");
            PravimFS();
            do
            {
                Console.Write("root/" + ": ");
                string linija = Console.ReadLine();
                string[] rijeci = linija.Split(' ');
                switch (rijeci[0])
                {
                    case "exit":
                        {
                            Environment.Exit(0);
                            break;
                        }
                    case "clr":
                        {
                            Console.Clear();
                            break;
                        }
                    case "cd":
                        {
                            if (rijeci.Count() == 2 && rijeci[1] != "..")
                                OtvoriDirektorijum(rijeci[1]);
                            else if (rijeci.Count() == 2 && rijeci[1] == "..")
                                Console.WriteLine("Greska, nalazite se u root direktorijumu!");
                            else
                                Console.WriteLine("Nepravilan poziv funkcije!");
                            break;
                        }
                    case "mkdir":
                        {
                            if (rijeci.Count() == 2)
                                KreirajDirektorijum(rijeci[1]);
                            else Console.WriteLine("Nepravilan poziv funkcije.");
                            break;
                        }
                    case "create":
                        {
                            if (rijeci.Count() == 2)
                                KreirajDatoteku(rijeci[1]);
                            else Console.WriteLine("Nepravilan poziv funkcije.");
                            break;
                        }
                    case "ls":
                        {
                            if (rijeci.Count() == 1)
                                SadrzajDirektorijuma();
                            else Console.WriteLine("Nepravilan poziv funkcije.");
                            break;
                        }
                    case "cp":
                        {
                            if (rijeci.Count() == 2)
                                KopirajDatoteku(rijeci[1]);
                            else Console.WriteLine("Nepravilan poziv funkcije.");
                            break;
                        }
                    case "rename":
                        {
                            if (rijeci.Count() == 3)
                                Preimenuj(rijeci[1], rijeci[2]);
                            else Console.WriteLine("Nepravilan poziv funkcije.");
                            break;
                        }
                    case "mv": //Ne radii!!!//
                        {
                            if (rijeci.Count() == 3)
                                MoveFile(rijeci[1], rijeci[2]); // rijeci[1]=fajl, rijeci[2]=odrediste
                            else
                                Console.WriteLine("Nepravilan poziv funkcije.");
                            break;
                        }
                    case "echo":
                        {
                            if (rijeci.Count() == 2)
                            {
                                UpisiUFajl(rijeci[1]);
                            }
                            else
                            {
                                Console.WriteLine("Nepravilan poziv funkcije.");
                            }
                            break;
                        }
                    case "cat":
                        {
                            if (rijeci.Count() == 2)
                            {
                                CitajIzFajla(rijeci[1]);
                            }
                            else
                            {
                                Console.WriteLine("Nepravilan poziv funkcije.");
                            }
                            break;
                        }
                    case "stat":
                        {
                            if (rijeci.Count() == 2)
                            {
                                InformacijeOFajlu(rijeci[1]);
                            }
                            else
                            {
                                Console.WriteLine("Nepravilan poziv funkcije.");
                            }
                            break;
                        }
                    case "rm":
                        {
                            if (rijeci.Count() == 2)
                            {
                                ObrisiDatoteku(rijeci[1]);
                            }
                            else if (rijeci.Count() == 3 && rijeci[1] == "-r")
                            {
                                ObrisiDirektorjium(rijeci[2]);
                            }
                            else
                            {
                                Console.WriteLine("Nepravilan poziv funkcije.");
                            }
                            break;
                        }
                    case "put":
                        {
                            if (rijeci.Count() == 1)
                            {
                                PutFile();
                            }
                            else
                            {
                                Console.WriteLine("Nepravilan poziv funkcije.");
                            }
                            break;
                        }
                    case "get":
                        {
                            if (rijeci.Count() == 1)
                            {
                                GetFile();
                            }
                            else
                            {
                                Console.WriteLine("Nepravilan poziv funkcije.");
                            }
                            break;
                        }
                    default:
                        Console.WriteLine("Nepoznata opcija.");
                        break;

                }

            }
            while (true);
        }

        private void PravimFS()
        {
            if (!File.Exists("FajlSistem.bin"))
            {
                FileStream fajl = new FileStream("FajlSistem.bin", FileMode.OpenOrCreate);
                StreamWriter pisalo = new StreamWriter(fajl);
                pisalo.Write("FSendzi" + " " + brDirektorijuma + " " + brDatoteka + " " + iskoriscenProstor + " " + slobodanProstor + " " + velicinaFS + '\n');
                pisalo.Write(separator);
                pisalo.Write('\n');
                pisalo.Close();
                fajl.Close();
            }
            else
            {
                FileStream fajl = new FileStream("FajlSistem.bin", FileMode.OpenOrCreate);
                StreamReader citalo = new StreamReader(fajl);
                string prvaLinija = citalo.ReadLine();
                string[] dijelovi = prvaLinija.Split(' ');
                brDirektorijuma = Convert.ToInt32(dijelovi[1]);
                brDatoteka = Convert.ToInt32(dijelovi[2]);
                iskoriscenProstor = Convert.ToInt32(dijelovi[3]);
                citalo.Close();
                fajl.Close();
            }
        }

        private void KreirajDirektorijum(string putanja)
        {
            if (iskoriscenProstor > velicinaFS)
            {
                Console.WriteLine("Memorija puna.");
                return;
            }
            if (ProvjeraPostojanja(putanja))
            {
                Console.WriteLine("Direktorijum vec postoji.");
                return;
            }
            putanja = Provjera(putanja);
            int ID = DodjelaID();
            UpisiMFTzapis(putanja, ID, 'd');
            brDirektorijuma += 1;
            AzurirajPrvuLiniju();
        }

        private void KreirajDatoteku(string putanja)
        {
            if (iskoriscenProstor > velicinaFS)
            {
                Console.WriteLine("Memorija puna.");
                return;
            }
            if (PostojiDatoteka(putanja))
            {
                Console.WriteLine("Fajl sa istim nazivom vec postoji.");
                return;
            }
            putanja = ProvjeraDatoteke(putanja);
            int ID = DodjelaID();
            UpisiMFTzapis(putanja, ID, 'f');
            brDatoteka += 1;
            AzurirajPrvuLiniju();
        }

        private string Provjera(string putanja)
        {
            bool fleg = false;
            while (!fleg)
            {
                if (putanja.StartsWith("root/"))
                {
                    string naziv = putanja.Substring(5);
                    if (!ProvjeraNaziva(naziv))
                    {
                        Console.WriteLine("Greska. Ponovo unesite putanju:");
                        putanja = Console.ReadLine();
                    }
                    else
                        fleg = true;
                }
                else
                {
                    Console.WriteLine("Greska. Nepravilno unesena putanja. Ponovo unesite putanju.");
                    putanja = Console.ReadLine();
                }
            }
            return putanja;
        }

        private string ProvjeraDatoteke(string putanja)
        {
            bool fleg = false;
            while (!fleg)
            {
                if (putanja.StartsWith("root/"))
                {
                    string naziv = putanja.Substring(5);
                    Regex regex = new Regex(@"[a-zA-Z0-9/]+");
                    Match match = regex.Match(naziv);
                    if (naziv.StartsWith(match.Value + '.'))
                    {
                        if (!ProvjeraNaziva(naziv))
                        {
                            Console.WriteLine("Greska. Ponovo unesite putanju:");
                            putanja = Console.ReadLine();
                        }
                        else
                            fleg = true;
                    }
                    else
                    {
                        Console.WriteLine("Greska. Fajl ne sadrzi ekstenziju.");
                        putanja = Console.ReadLine();
                    }
                }
                else
                {
                    Console.WriteLine("Greska. Nepravilno unesena putanja. Ponovo unesite putanju.");
                    putanja = Console.ReadLine();
                }
            }
            return putanja;
        }

        private bool ProvjeraNaziva(string naziv)
        {
            bool fleg = false;
            Regex regex = new Regex(@"[a-zA-Z0-9-./ ]+");
            Match match = regex.Match(naziv);
            if (naziv.Equals(match.Value))
            {
                fleg = true;
                return fleg;
            }
            return fleg;
        }

        private int DodjelaID()
        {
            StreamReader citalo = new StreamReader(new FileStream("ID.txt", FileMode.OpenOrCreate));
            string linija = citalo.ReadLine();
            int ID = Convert.ToInt32(linija) + 1;
            citalo.Close();
            StreamWriter pisalo = new StreamWriter(new FileStream("ID.txt", FileMode.Open));
            pisalo.WriteLine(ID);
            pisalo.Close();
            return ID;
        }

        private void UpisiMFTzapis(string putanja, int ID, char x)
        {
            byte[] content = File.ReadAllBytes("FajlSistem.bin");
            int start = 0;
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '\n')
                {
                    start = i + 1;
                    break;
                }
            }

            BinaryWriter pisalo1 = new BinaryWriter(new FileStream("FajlSistem.bin", FileMode.Truncate));
            for (int i = 0; i < start; i++)
                pisalo1.Write(content[i]);
            pisalo1.Close();

            if (x == 'f')
            {
                StreamWriter pisalo2 = new StreamWriter(new FileStream("FajlSistem.bin", FileMode.Append));
                pisalo2.Write(x + "***" + ID + "***" + putanja + "***" + DateTime.Now + "|" + initialSize + '\n');
                pisalo2.Close();
            }
            else
            {
                StreamWriter pisalo2 = new StreamWriter(new FileStream("FajlSistem.bin", FileMode.Append));
                pisalo2.Write(x + "***" + ID + "***" + putanja + "***" + DateTime.Now + '\n');
                pisalo2.Close();
            }

            BinaryWriter pisalo3 = new BinaryWriter(new FileStream("FajlSistem.bin", FileMode.Append));
            for (int i = start; i < content.Length; i++)
                pisalo3.Write(content[i]);
            pisalo3.Close();
        }

        private void AzurirajPrvuLiniju()
        {
            byte[] content = File.ReadAllBytes("FajlSistem.bin");
            int start = 0;
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '\n')
                {
                    start = i + 1;
                    break;
                }
            }

            glavniIskoriscenProstor = content.Length;
            glavniSlobodanProstor = velicinaFS - glavniIskoriscenProstor;

            StreamWriter pisalo1 = new StreamWriter(new FileStream("FajlSistem.bin", FileMode.Truncate));
            pisalo1.Write("FSendzi" + " " + brDirektorijuma + " " + brDatoteka + " " + glavniIskoriscenProstor + " " + glavniSlobodanProstor + " " + velicinaFS + '\n');
            pisalo1.Close();

            BinaryWriter pisalo2 = new BinaryWriter(new FileStream("FajlSistem.bin", FileMode.Append));
            for (int i = start; i < content.Length; i++)
                pisalo2.Write(content[i]);
            pisalo2.Close();
        }

        private bool PostojiDatoteka(string putanja)
        {
            bool fleg = false;

            StreamReader citalo = new StreamReader(new FileStream("FajlSistem.bin", FileMode.OpenOrCreate));
            string linija = citalo.ReadLine();
            Regex regex = new Regex(@"[A-Za-z0-9/ :.-]+");
            while (linija != separator)
            {
                MatchCollection matches = regex.Matches(linija);
                if (matches[0].Value == "f" && linija.Contains("***" + putanja + "***"))
                {
                    fleg = true;
                    break;
                }
                linija = citalo.ReadLine();
            }
            citalo.Close();
            return fleg;
        }

        private bool ProvjeraPostojanja(string ime)
        {
            bool fleg = false;

            StreamReader citalo = new StreamReader(new FileStream("FajlSistem.bin", FileMode.OpenOrCreate));
            string linija = citalo.ReadLine();
            Regex regex = new Regex(@"[A-Za-z0-9/ :.-]+");
            while (linija != separator)
            {
                MatchCollection matches = regex.Matches(linija);
                if (matches[0].Value == "d" && linija.Contains("***" + ime + "***"))
                {
                    fleg = true;
                    break;
                }
                linija = citalo.ReadLine();
            }
            citalo.Close();
            return fleg;
        }

        private void SadrzajDirektorijuma(string putanja = "root/")
        {
            StreamReader citalo = new StreamReader(new FileStream("FajlSistem.bin", FileMode.Open));
            string preskociPrvuLiniju = citalo.ReadLine();

            Console.WriteLine("Tip/////////Putanja/////////Datum kreiranja/////////");

            Regex regex = new Regex(@"[a-zA-Z0-9/ .-:]+");
            string linija = citalo.ReadLine();

            while (!linija.Contains(separator))
            {
                MatchCollection matches = regex.Matches(linija);

                if (putanja.Equals("root/") && matches[2].Value == (new Regex(@"root/([A-Za-z0-9.-]+)")).Match(matches[2].Value).Value)
                {
                    Console.WriteLine(matches[0].Value + "---" + matches[1].Value + " --- " + matches[2].Value + " --- " + matches[3].Value + " --- ");
                }

                else if (!putanja.Equals("root/") && matches[2].Value.Contains(putanja))
                {
                    Console.WriteLine(matches[0].Value + " --- " + matches[1].Value + " --- " + matches[2].Value + " --- " + matches[3].Value);
                }
                linija = citalo.ReadLine();
            }

            citalo.Close();
        }

        private void KopirajDatoteku(string name, string path = "root/")
        {
            if (!PostojiDatoteka(path + name))
            {
                Console.WriteLine("Greska. Ne postoji trazeni fajl.");
                return;
            }

            string newName = "";
            string[] partsOfName = name.Split('.'); 
            for (int i = 1; i < 10; i++) 
            {
                newName = partsOfName[0] + "Copy" + i.ToString() + "." + partsOfName[1]; //novi naziv je oblika nazivCopy1.txt
                if (!PostojiDatoteka(path + newName))
                    break;
            }

            KreirajDatoteku(path + newName);

            if (GetSizeOfFile(name) != 0)
            {
                byte[] contentOfFile = CitajIzDataSegmenta(GetID(name)); 

                if (!UpisiUDataSegment(GetID(newName), contentOfFile))
                {
                    Console.WriteLine("Greska. nema dovoljno memorije na fajl sistemu.");
                    return;
                }

                UpdateSizeOfFile(newName, path, contentOfFile.Length);
            }
        }

        private void Preimenuj(string staroIme, string novoIme)
        {
            if (!PostojiDatoteka("root/" + staroIme))
            {
                Console.WriteLine("Greska! Ne postoji taj fajl na zadatoj putanji!");
                return;
            }

            while (!ProvjeraNaziva(novoIme) && !PostojiDatoteka("root/" + novoIme))
            {
                Console.WriteLine("Novi naziv nije odgovarajuci. Ponovite unos novog naziva:");
                novoIme = Console.ReadLine();
            }

            byte[] _id = GetID(staroIme);

            int i = 0;
            byte[] _novoIme = new byte[novoIme.Length];
            foreach (var x in novoIme)
                _novoIme[i++] = (byte)x;

            int start1 = 0, end1 = 0;
            byte[] contentOfFS = File.ReadAllBytes("FajlSistem.bin");
            for (i = 0; i < contentOfFS.Length; i++)
            {
                if (contentOfFS[i] == '*' && contentOfFS[i + 1] == '*' && contentOfFS[i + 2] == '*' && contentOfFS[i + 3] == _id[0] && contentOfFS[i + 4] == _id[1] && contentOfFS[i + 5] == _id[2])
                {
                    start1 = i + 14;
                    end1 = start1 + staroIme.Length + 1;
                    break;
                }
            }

            List<byte> newContentOfFS = new List<byte>();
            for (i = 0; i < start1; i++)
            {
                newContentOfFS.Add(contentOfFS[i]);
            }
            for (i = 0; i < _novoIme.Length; i++)
            {
                newContentOfFS.Add(_novoIme[i]);
            }
            for (i = end1; i < contentOfFS.Length; i++)
            {
                newContentOfFS.Add(contentOfFS[i]);
            }

            PrepisiFS(newContentOfFS.ToArray());

            AzurirajPrvuLiniju(); AzurirajPrvuLiniju();
        }

        private (string, string) RazdvojiPutanju(string putanjaZaProvjeriti)
        {
            string putanja = "", ime = "";
            if (putanjaZaProvjeriti.StartsWith("root/")) 
            {
                string[] parts = putanjaZaProvjeriti.Split('/');
                if (parts.Count() == 2)  
                {
                    putanja = parts[0];
                    ime = parts[1];
                }
                else if (parts.Count() == 3)
                {
                    putanja = parts[0] + "/" + parts[1];
                    ime = parts[2];
                }
            }
            return (putanja, ime);
        }

        private void PrepisiFS(byte[] content)
        {
            BinaryWriter writer = new BinaryWriter(new FileStream("FajlSistem.bin", FileMode.Truncate));
            writer.Write(content);
            writer.Close();
        }

        private byte[] GetID(string ime)
        {
            StreamReader citalo = new StreamReader(new FileStream("FajlSistem.bin", FileMode.Open));
            string linija = citalo.ReadLine();
            while (!linija.Contains(separator))
            {
                if (linija.Contains(ime))
                {
                    Regex regex = new Regex(@"[0-9]+");
                    Match match = regex.Match(linija);
                    string IDstring = match.ToString();
                    citalo.Close();

                    byte[] _id = new byte[IDstring.Length];
                    for (int i = 0; i < IDstring.Length; i++)
                        _id[i] = (byte)IDstring[i];

                    return _id;
                }
                else
                    linija = citalo.ReadLine();
            }
            byte[] error = { (byte)'-', (byte)'1' };
            return error;
        }

        private void PremjestiDatoteku(string datoteka, string odrediste = "root/")
        {
            if (!datoteka.Contains("/"))
            {
                if (!PostojiDatoteka("root/" + datoteka))
                {
                    Console.WriteLine("Trazeni fajl ne postoji.");
                    return;
                }
                else if (!ProvjeraPostojanja("root/" + odrediste))
                {
                    Console.WriteLine("Nema tog odredista.");
                }
                else
                {
                    KreirajDirektorijum("root/" + odrediste + "/" + datoteka);
                    ObrisiDatoteku("root/" + datoteka);
                }
            }
            else if (datoteka.Contains("/"))
            {
                string putanja, ime;
                string razdvoji = "root/" + datoteka;
                (putanja, ime) = RazdvojiPutanju(razdvoji);
                if (!ProvjeraPostojanja("root/" + odrediste))
                {
                    Console.WriteLine("Nema tog odredista.");
                }
                else
                {
                    KreirajDirektorijum("root/" + odrediste + "/" + ime);
                    ObrisiDatoteku(datoteka);
                }
            }
            AzurirajPrvuLiniju(); AzurirajPrvuLiniju();
        } // Neeeeeee radiiiiiii :(

        private void UpisiUFajl(string name, string path = "root/")
        {
            if (!PostojiDatoteka(path + name))
            {
                Console.WriteLine("Greska. Ne postoji trazeni fajl na trenutnoj putanji");
                return;
            }

            if (!name.EndsWith(".txt"))
            {
                Console.WriteLine("Greska. Echo radi samo sa tekstulanim fajlovima.");
                return;
            }

            Console.WriteLine("Unesite sadrzaj fajla:");
            string sContentOfFile = Console.ReadLine();
            byte[] _contentOfFile = new byte[sContentOfFile.Length];
            for (int i = 0; i < sContentOfFile.Length; i++)
                _contentOfFile[i] = (byte)sContentOfFile[i];
            List<byte> contentOfFile = _contentOfFile.ToList();

            byte[] _id = GetID(name);

            int initialSize = GetSizeOfFile(name);
            int newSize = initialSize + sContentOfFile.Length;
            if (newSize > velicinaFS)
            {
                Console.WriteLine("Greska. Maksimalna dozvoljena velicina fajla je 64kB");
                return;
            }
            if (initialSize == 0)
            {
                if (!UpisiUDataSegment(_id, _contentOfFile))
                    return;
            }
            else if(initialSize > 0)
            {
                if(contentOfFile.Count > glavniSlobodanProstor)
                {
                    Console.WriteLine("Greska. Memorija je puna.");
                    return;
                }

                //Console.WriteLine("prije prob.");

                byte[] _stariSadrzajFajla = CitajIzDataSegmenta(_id);
                List<byte> stariSadrzajFajla = _stariSadrzajFajla.ToList();

                ObrisiIzDataSegmenta(_id);

                stariSadrzajFajla.AddRange(contentOfFile);

                byte[] noviSadrzajFajla = stariSadrzajFajla.ToArray();
                UpisiUDataSegment(_id, noviSadrzajFajla);
            }
            UpdateSizeOfFile(name, path, newSize);

            AzurirajPrvuLiniju();
        }

        private int GetSizeOfFile(string ime)
        {
            StreamReader citalo = new StreamReader(new FileStream("FajlSistem.bin", FileMode.Open));
            string linija = citalo.ReadLine();
            while (!linija.Contains(separator))
            {
                if (linija.Contains("***root/" + ime + "***"))
                {
                    string velicina = linija.Split('|')[1];
                    citalo.Close();
                    return Convert.ToInt32(velicina);
                }
                linija = citalo.ReadLine();
            }
            citalo.Close();
            return -1;
        }

        private bool UpisiUDataSegment(byte[] _id, byte[] sadrzaj)
        {
            byte[] stariSadrzaj = File.ReadAllBytes("FajlSistem.bin");
            int mem = sadrzaj.Length + _id.Length + 8;
            //Console.WriteLine("Stanje slobodnog prostora:" + mem);
            if(sadrzaj.Length + _id.Length + 8 > slobodanProstor)
            {
                Console.WriteLine("Greska. Memorija puna.");
                return false;
            }

            BinaryWriter pisalo = new BinaryWriter(new FileStream("FajlSistem.bin", FileMode.Truncate));
            pisalo.Write(stariSadrzaj);
            pisalo.Write((byte)'*');
            pisalo.Write(_id);
            pisalo.Write((byte)'I');
            pisalo.Write((byte)'*');
            pisalo.Write(sadrzaj);
            pisalo.Write((byte)'*');
            pisalo.Write((byte)'E');
            pisalo.Write((byte)'O');
            pisalo.Write((byte)'F');
            pisalo.Write((byte)'*');

            pisalo.Close();

            AzurirajPrvuLiniju();
            return true;

        }

        private byte[] CitajIzDataSegmenta(byte[] _id)
        {
            byte[] _contentOfFS = File.ReadAllBytes("FajlSistem.bin");
            int start, end;
            (start, end) = GetStartAndEndPositions(_id);

            LinkedList<byte> contentOfFile = new LinkedList<byte>();
            for(int i = start; i <= end; i++)
            {
                contentOfFile.AddLast(_contentOfFS[i]);
            }

            return contentOfFile.ToArray();
        }

        private void ObrisiIzDataSegmenta(byte[] _id)
        {
            byte[] _contentOfFS = File.ReadAllBytes("FajlSistem.bin");

            int start, end;
            (start, end) = GetStartAndEndPositions(_id);

            LinkedList<byte> newContentOfFS = new LinkedList<byte>();
            for (int i = 0; i < start - 6; ++i)
                newContentOfFS.AddLast(_contentOfFS[i]);
            for (int i = end + 6; i < _contentOfFS.Length; ++i)
                newContentOfFS.AddLast(_contentOfFS[i]);

            byte[] _newContentOfFS = newContentOfFS.ToArray();

            PrepisiFS(_newContentOfFS);

            AzurirajPrvuLiniju();
        }

        private (int, int) GetStartAndEndPositions(byte[] _id)
        {
            byte[] _contentOfFS = File.ReadAllBytes("FajlSistem.bin");
            int start = 0, end = 0;
            for (int i = 0; i < _contentOfFS.Length; i++)
            {
                if (_contentOfFS[i] == '*' && _contentOfFS[i + 1] == _id[0] && _contentOfFS[i + 2] == _id[1] && _contentOfFS[i + 3] == _id[2] && _contentOfFS[i + 4] == 'I' && _contentOfFS[i + 5] == '*')
                {
                    start = i + 6;
                    for (int j = start; j < _contentOfFS.Length; ++j)
                    {
                        if (_contentOfFS[j + 1] == '*' && _contentOfFS[j + 2] == 'E' && _contentOfFS[j + 3] == 'O' && _contentOfFS[j + 4] == 'F' && _contentOfFS[j + 5] == '*')
                        {
                            end = j;
                            break;
                        }
                    }
                    break;
                }
            }
            return (start, end);
        }

        private void UpdateSizeOfFile(string name, string path, int newSize)
        {
            byte[] _id = GetID(name);

            int i = 0;
            string newSizeS = newSize.ToString();
            byte[] _newSize = new byte[newSizeS.Length];
            foreach (var x in newSizeS)
                _newSize[i++] = (byte)x;

            int index1 = 0, index2 = 0;
            byte[] contentOfFS = File.ReadAllBytes("FajlSistem.bin");
            for (i = 0; i < contentOfFS.Length; i++)
            {
                if (contentOfFS[i] == '*' && contentOfFS[i + 1] == _id[0] && contentOfFS[i + 2] == _id[1] && contentOfFS[i + 3] == _id[2] && contentOfFS[i + 4] == '*')
                {
                    int br = 0;
                    for (int j = i + 1; j < contentOfFS.Length; j++) 
                    {
                        if (contentOfFS[j] == '|')
                        {
                            br++;
                        }
                        if (br == 1) 
                        {
                            index1 = j + 1;
                            for (int k = index1; k < contentOfFS.Length; k++)
                            {
                                if (contentOfFS[k] == '\n')
                                {
                                    index2 = k;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
            }

            List<byte> newContentOfFS = new List<byte>();
            for (i = 0; i < index1; i++)
            {
                newContentOfFS.Add(contentOfFS[i]);
            }
            for (i = 0; i < _newSize.Length; i++)
            {
                newContentOfFS.Add(_newSize[i]);
            }
            for (i = index2; i < contentOfFS.Length; i++)
            {
                newContentOfFS.Add(contentOfFS[i]);
            }

            PrepisiFS(newContentOfFS.ToArray());

            AzurirajPrvuLiniju();
        }

        private void CitajIzFajla(string name, string path = "root/")
        {
            if(!PostojiDatoteka("root/" + name))
            {
                Console.WriteLine(("Greska. Ne postoji fajl na trenutnoj putanji."));
                return;
            }
            if(!name.EndsWith(".txt"))
            {
                Console.WriteLine("Greska. Komanda cat radi samo sa tekstualnim fajlovima.");
                return;
            }
            if (GetSizeOfFile(name) == 0)
                Console.WriteLine("Fajl je prazna.");
            else
            {
                byte[] _id = GetID(name);
                byte[] contentOfFile = CitajIzDataSegmenta(_id);
                char[] sadrzaj = new char[contentOfFile.Length];

                for (int i = 0; i < contentOfFile.Length; i++)
                    sadrzaj[i] = (char)contentOfFile[i];

                sadrzaj.ToString();
                Console.WriteLine(sadrzaj);

                Console.WriteLine('\n');
            }
        }

        private void InformacijeOFajlu(string ime1, string putanja1 = "root/")
        {
            string putanja, ime, putanjaA;
            (putanjaA, ime) = RazdvojiPutanju(putanja1 + ime1);
            putanja = putanjaA + '/';
            if (!PostojiDatoteka(putanja + ime))
            {
                Console.WriteLine("Greska. Ne postoji trazeni fajl na trenutnoj putanji");
                return;
            }
            if (GetType(ime, putanja) == 'd')
            {
                Console.WriteLine("Greska. Stat radi samo sa fajlovima.");
                return;
            }

            byte[] _id = GetID(ime1);
            int start;
            (start, _) = GetStartAndEndPositions(_id);

            Console.WriteLine("***Naziv datoteke: " + ime + " ***");
            Console.Write("***ID: ");
            for (int i = 0; i < _id.Length; i++)
                Console.Write((char)_id[i]);
            Console.Write(" ***");
            Console.WriteLine("\n***Putanja: " + putanja + ime + " ***");
            string datum = GetDateCreated(ime, putanja);
            Console.WriteLine("***Datum i vrijeme kreiranja: " + datum + " ***");
            Console.WriteLine("***Velicina: " + GetSizeOfFile(ime1) + "b ***");
            Console.WriteLine("***Broj blokova: {0}", Math.Ceiling((double)GetSizeOfFile(ime1) / (double)velicinaBloka), " ***");
            Console.WriteLine("**Pocetna lokacija blokova: {0}", start, " ***");
        }

        private char GetType(string name, string path)
        {

            char type = 'x'; //u slucaju da ne pronadje trazini fajl, vratice x
            StreamReader reader = new StreamReader(new FileStream("FajlSistem.bin", FileMode.Open));
            string line = reader.ReadLine();
            while (!line.Contains(separator))
            {
                if (line.Contains("***" + path + name + "***"))
                {
                    type = line[0];
                    break;
                }
                line = reader.ReadLine();
            }
            reader.Close();
            return type;
        }

        private string GetDateCreated(string name, string path)
        {
            StreamReader reader = new StreamReader(new FileStream("FajlSistem.bin", FileMode.Open));
            string line = reader.ReadLine();
            while (!line.Contains(separator))
            {
                if (line.Contains("***" + path + name + "***"))
                {
                    string id = (line.Split('*'))[9];
                    reader.Close();
                    return id;
                }
                line = reader.ReadLine();
            }
            return "-1";
        }

        private void RemoveMftRecord(char type, string name, string path = "root/")
        {
            byte[] contentOfFS = File.ReadAllBytes("FajlSistem.bin");

            byte[] _id = GetID(name);

            int start = 0, end = 0;
            for (int i = 0; i < contentOfFS.Length; i++)
            {
                if (contentOfFS[i] == '*' && contentOfFS[i + 1] == _id[0] && contentOfFS[i + 2] == _id[1] && contentOfFS[i + 3] == _id[2] && contentOfFS[i + 4] == '*')
                {
                    if (type == 'd')
                    {
                        start = i - 4; //početak linije koju treba obrisati
                        break;
                    }
                    else if (type == 'f')
                    {
                        start = i - 4; //početak linije koju treba obrisati
                        break;
                    }
                }
            }
            for (int i = start + 1; i < contentOfFS.Length; ++i)
            {
                if (contentOfFS[i] == (byte)'\n')
                {
                    end = i;
                    break;
                } //prvi znak za novi red je kraj linije koju brisemo
            }

            List<byte> newContentOfFS = new List<byte>();
            for (int i = 0; i < start; ++i)
            {
                newContentOfFS.Add(contentOfFS[i]);
            }
            for (int i = end; i < contentOfFS.Length; ++i)
            {
                newContentOfFS.Add(contentOfFS[i]);
            }

            PrepisiFS(newContentOfFS.ToArray());
        }

        private void ObrisiDatoteku(string name, string path = "root/")
        {
            if (!PostojiDatoteka(path + name))
            {
                Console.WriteLine("Greska. Ne postoji trazeni fajl..", name);
                return;
            }

            byte[] _id = GetID(name);

            if (GetSizeOfFile(name) != 0)
            {
                ObrisiIzDataSegmenta(_id);
            }

            RemoveMftRecord('f', name, path);

            brDatoteka--;

            AzurirajPrvuLiniju();

        }

        private void ObrisiDirektorjium(string name, string path = "root/")
        {
            if (name.StartsWith("root/"))
            {
                name = name.Substring(5);
            }

            if (!ProvjeraPostojanja(path + name))
            {
                Console.WriteLine("Greska - ne postoji trazeni direktorijum.");
                return;
            }

            string[] files = GetFilesOfDirectory(name);
            foreach (var file in files)
                ObrisiDatoteku(file, path + name + "/");

            RemoveMftRecord('d', name);

            brDirektorijuma--;

            AzurirajPrvuLiniju();

        }

        private string[] GetFilesOfDirectory(string nameOfDirectory)
        {
            LinkedList<string> files = new LinkedList<string>();

            StreamReader reader = new StreamReader(new FileStream("FajlSistem.bin", FileMode.Open));
            string line = reader.ReadLine();
            while (!line.Contains(separator))
            {
                if (line.Contains("***root/" + nameOfDirectory + "/"))
                {
                    files.AddLast(line.Split('*')[6]);
                }
                line = reader.ReadLine();
            }
            reader.Close();

            return files.ToArray();
        }

        private void MoveFile(string name, string newPathh, string path = "root/")
        {
            string newPath = "root/" + newPathh;
            if (!PostojiDatoteka(path + name))
            {
                Console.WriteLine("Greska. Ne postoji trazeni fajl na trenutnoj putanji", name);
                return;
            }

            if (!path.Equals("root/")) 
            {
                string tmpPath, tmpName;
                (tmpPath, tmpName) = RazdvojiPutanju(path.Substring(0, path.Length - 1));
                if (tmpName == "" || tmpPath == "")
                {
                    Console.WriteLine("Greska. Unesena je nepostojeca putanja.");
                    return;
                }
                if (!PostojiDatoteka(tmpName))
                {
                    Console.WriteLine("Greska. Pokusavate dodati u folder koji ne postoji.");
                    return;
                }
            }

            if (PostojiDatoteka(newPath + name))
            {
                Console.WriteLine("Greska. Fajl vec postoji.");
                return;
            }

            byte[] _id = GetID(name);

            int i = 0;
            byte[] _newPath = new byte[newPath.Length];
            foreach (var x in newPath)
                _newPath[i++] = (byte)x;

            int start = 0, end = 0;
            byte[] contentOfFS = File.ReadAllBytes("FajlSistem.bin");
            for (i = 0; i < contentOfFS.Length; i++)
            {
                if (contentOfFS[i] == '*' && contentOfFS[i + 1] == _id[0] && contentOfFS[i + 2] == _id[1] && contentOfFS[i + 3] == _id[2] && contentOfFS[i + 4] == '*')
                {
                    start = i + path.Length + 6;
                    end = start + (path + name).Length;
                    break;
                }
            }

            List<byte> newContentOfFS = new List<byte>();
            for (i = 0; i < start; i++)
            {
                newContentOfFS.Add(contentOfFS[i]);
            }
            for (i = 0; i < _newPath.Length; i++)
            {
                newContentOfFS.Add(_newPath[i]);
            }
            for (i = end; i < contentOfFS.Length; i++)
            {
                newContentOfFS.Add(contentOfFS[i]);
            }

            PrepisiFS(newContentOfFS.ToArray());

            AzurirajPrvuLiniju();
        }

        private void PutFile()
        {
            Console.WriteLine(@"Unesite putanju ulaznog fajla(C:\\Users\\andje\\Desktop\\dat.txt):");
            string inputPath = Console.ReadLine();
            Console.WriteLine("Unesite putanju izlaznog fajla(primjer: root/folder/datoteka):");
            string outputPath = Console.ReadLine();
            string inputDat = inputPath.Substring(27);
            Console.WriteLine("Substring je:" + inputDat);
            if (!File.Exists(inputPath))
            {
                Console.WriteLine("Greska - fajl {0} ne postoji.", inputPath);
                return;
            }

            string oPath, oFile;
            (oPath, oFile) = RazdvojiPutanju(outputPath);
            string path = oPath + '/';
            if (oPath == "" && oFile == "")
            {
                Console.WriteLine("Greska - unesena je nevazeca putanja {0}", outputPath);
                return;
            }

            if (PostojiDatoteka(path + oFile))
            {
                Console.WriteLine("Greska. Fajl vec postoji.", outputPath);
                return;
            }

            if ((path) != "root/")
            {
                string tmpPath, tmpName;
                (tmpPath, tmpName) = RazdvojiPutanju(oPath);
                if (!PostojiDatoteka(tmpPath + tmpName))
                {
                    Console.WriteLine("Greska. Trazeni folder ne postoji.");
                    return;
                }
            }

           

            List<byte> list = new List<byte>();
            try
            {
                byte[] content = File.ReadAllBytes(inputPath);
                list = content.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine("Doslo je do greske prilikom otvaranja fajla {0}", inputPath);
                Console.WriteLine(e.Message);
                return;
            }

            byte[] _contentOfInputFile = list.ToArray();

            if (_contentOfInputFile.Count() > velicinaFS)
            {
                Console.WriteLine("Greska. Memorija FS puna.)", velicinaFS);
                return;
            }

            KreirajDatoteku(path + oFile + '/' + inputDat);
            byte[] _id = GetID(oFile);

            if (!UpisiUDataSegment(_id, _contentOfInputFile))
            {
                Console.WriteLine("Greska. Memorija fajla puna.");
                return;
            }

            UpdateSizeOfFile(oFile, oPath, _contentOfInputFile.Length);

            AzurirajPrvuLiniju();
        }

        private void GetFile()
        {
            Console.WriteLine(@"Unesite putanju ulaznog fajla(sa pocetkom 'root/':");
            string inputPath = Console.ReadLine();
            Console.WriteLine(@"Unesite putanju izlaznog fajla(C:\\Users\\andje\\Desktop\\dat.txt):");
            string outputPath = Console.ReadLine();

            string iPath, iFile;
            (iPath, iFile) = RazdvojiPutanju(inputPath);
            string path = iPath + '/';
            if (iPath == "" && iFile == "")
            {
                Console.WriteLine("Greska - unesena je nevazeca putanja {0}", inputPath);
                return;
            }

            if (!PostojiDatoteka(path + iFile))
            {
                Console.WriteLine("Greska - fajl {0} nije pronadjen na fajl sistemu", inputPath);
                return;
            }

            byte[] contentOfFile = CitajIzDataSegmenta(GetID(iFile));

            try
            {
                File.WriteAllBytes(outputPath, contentOfFile);
            }
            catch (Exception e)
            {
                Console.WriteLine("Doslo je do greske prilikom upisivanja u fajl {0}", outputPath);
                Console.WriteLine(e.Message);
            }

        }

        private string CheckName(string name, string path, char type)
        {
            while (true)
            {
                Match match;
                if (type == 'd')
                    match = (new Regex(@"[A-Za-z0-9-]+")).Match(name);
                else
                    match = (new Regex(@"[A-Za-z0-9-]+\.[A-Za-z0-9]+")).Match(name);

                bool exists = PostojiDatoteka(path + name);

                if (name.Length <= 20 && !exists && match.Value == name)
                {
                    return name;
                }
                else if (match.Value != name)
                {
                    Console.WriteLine("Greska. Fajl mora sadrzati ekstenziju. Unesite datoteku.");
                    name = Console.ReadLine();
                }
                else if (exists)
                {
                    Console.WriteLine("Greska. Fajl(direktorijum) postoji. Unesite novi naziv.");
                    name = Console.ReadLine();
                }
            }
        }

        private void OtvoriDirektorijum(string putanja)
        {
            string putanja1 = "root/" + putanja;

            if (!ProvjeraPostojanja(putanja1))
            {
                Console.WriteLine("Trazeni direktorijum ne postoji.");
            }
            else
            {
                while (ProvjeraPostojanja(putanja1))
                {
                    Console.Write(putanja1 + ": ");
                    string linija = Console.ReadLine();
                    string[] rijeci = linija.Split(' ');

                    switch (rijeci[0])
                    {
                        case "exit":
                            Environment.Exit(0);
                            break;

                        case "clr":
                            Console.Clear();
                            break;

                        case "cd":
                            {
                                if (rijeci.Count() == 2 && rijeci[1] != "..")
                                {
                                    Console.WriteLine("Greska - za pozicioniranje u root folder unesite \"cd ..\"");
                                }
                                else if (rijeci.Count() == 2 && rijeci[1] == "..")
                                {
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine("Nepravilan poziv funkcije");
                                }
                                break;
                            }
                        case "mkdir":
                            {
                                if (rijeci.Count() == 2)
                                    KreirajDirektorijum(putanja1 + '/' + rijeci[1]);
                                else Console.WriteLine("Nepravilan poziv funkcije.");
                                break;
                            }
                        case "create":
                            {
                                if (rijeci.Count() == 2)
                                    KreirajDatoteku(putanja1 + '/' + rijeci[1]);
                                else Console.WriteLine("Nepravilan poziv funkcije.");
                                break;
                            }
                        case "ls":
                            {
                                if (rijeci.Count() == 1)
                                    SadrzajDirektorijuma(putanja1 + '/');
                                else Console.WriteLine("Nepravilan poziv funkcije.");
                                break;
                            }
                        case "cp":
                            {
                                if (rijeci.Count() == 2)
                                    KopirajDatoteku(putanja + '/' + rijeci[1]);
                                else Console.WriteLine("Nepravilan poziv funkcije.");
                                break;
                            }
                        case "rename":
                            {
                                if (rijeci.Count() == 3)
                                    Preimenuj(putanja + '/' + rijeci[1], putanja + '/' + rijeci[2]);
                                else Console.WriteLine("Nepravilan poziv funkcije.");
                                break;
                            }
                        case "mv":
                            {
                                if (rijeci.Count() == 3)
                                    Console.WriteLine("Trenutno nije moguce premjestanje, vratite se u root/.");
                                else
                                    Console.WriteLine("Nepravilan poziv funkcije.");
                                break;
                            }
                        case "echo":
                            {
                                if (rijeci.Count() == 2)
                                {
                                    UpisiUFajl(putanja + "/" + rijeci[1]);
                                }
                                else
                                {
                                    Console.WriteLine("Nepravilan poziv funkcije.");
                                }
                                break;
                            }
                        case "cat":
                            {
                                if (rijeci.Count() == 2)
                                {
                                    CitajIzFajla(putanja + "/" + rijeci[1]);
                                }
                                else
                                {
                                    Console.WriteLine("Nepravilan poziv funkcije.");
                                }
                                break;
                            }
                        case "stat":
                            {
                                if (rijeci.Count() == 2)
                                {
                                    InformacijeOFajlu(putanja + "/" + rijeci[1]);
                                }
                                else
                                {
                                    Console.WriteLine("Nepravilan poziv funkcije.");
                                }
                                break;
                            }
                        case "rm":
                            {
                                if (rijeci.Count() == 2)
                                {
                                    ObrisiDatoteku(putanja + "/" + rijeci[1]);
                                }
                                else if (rijeci.Count() == 3 && rijeci[1] == "-r")
                                {
                                    ObrisiDirektorjium(putanja + "/" + rijeci[2]);
                                }
                                else
                                {
                                    Console.WriteLine("Greska - Netacan unos! Unesite \"help rm\" za pomoc");
                                }
                                break;
                            }
                        case "put":
                            {
                                if (rijeci.Count() == 1)
                                {
                                    PutFile();
                                }
                                else
                                {
                                    Console.WriteLine("Nepravilan poziv funkcije.");
                                }
                                break;
                            }
                        case "get":
                            {
                                if (rijeci.Count() == 1)
                                {
                                    GetFile();
                                }
                                else
                                {
                                    Console.WriteLine("Nepravilan poziv funkcije.");
                                }
                                break;
                            }
                        default:
                            Console.WriteLine("Nepoznata opcija:");
                            break;
                    }
                }
            }
        }
    }
}
