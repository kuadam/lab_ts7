using System;
using System.Collections;
using System.Net.Sockets;

/*Operacje
 * 000001-żądanie nadania id
 * 000010-nadanie id przez serwer 
 * 000011-klient przesyla liczbe do przedzialu
 * 000100-serwer przesyla min przedzialu
 * 000101-serwer przesyla max przedzialu
 * 000110-klient przesyla zgadywana liczbe
 * 000111-serwer przesyla odpowiedz
     */

/*Odpowiedz
 * 001-wygrana
 * 010-za duza
 * 011-za mala
 * 100-przegrana
 * 101-remis
 */

namespace klient
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Wprowadz ip serwera");
            string ip;
            ip = Console.ReadLine();
            try
            {
                BitArray bitArray = new BitArray(24);
                Byte[] bytes = new Byte[3];
                int min = 0, max = 0;
                int port = 51997;
                TcpClient client = new TcpClient(ip, Convert.ToInt32(port));


                NetworkStream stream = client.GetStream();
                // Przeslanie zadania nadania id 

                Komunikat komunikat = new Komunikat();
                komunikat.SetOp("000001");
                komunikat.GetBitArray().CopyTo(bytes,0);
                stream.Write(bytes, 0, bytes.Length);

                int i;
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    komunikat.ustaw(bytes);
                    Console.WriteLine("ID nadane przez serwer: " + Convert.ToInt32(komunikat.GetId(), 2));
                    break;
                }


                //Rozpoczecie gry, przeslanie liczby
                komunikat.Resetruj();
                Console.WriteLine("\nWprowadz liczbe <0-200> aby rozpoczac gre: ");
                komunikat.SetOp("000011");
                //string temp;
                //int liczba;
                string temp;
                int liczba=0;
                bool czy_liczba;
                do
                {
                    czy_liczba = true;
                    temp = Console.ReadLine();
                    foreach (var x in temp)
                    {
                        if (!Char.IsDigit(x))
                        {
                            czy_liczba = false;
                            Console.WriteLine("Podaj liczbe z przedzialu 0-200");
                            break;
                        }
                    }
                    if (czy_liczba)
                    {
                        liczba = Convert.ToInt32(temp);
                        if (liczba > 200 || liczba < 0)
                        {
                            Console.WriteLine("Podaj liczbe z przedzialu 0-200");
                            czy_liczba = false;
                        }
                    }
                } while (!czy_liczba);

                komunikat.SetLiczba(Convert.ToString(liczba, 2).PadLeft(8, '0'));

                komunikat.GetBitArray().CopyTo(bytes, 0);
                stream.Write(bytes, 0, bytes.Length);

                //odbieranie przedzialu
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    komunikat.Resetruj();
                    komunikat.ustaw(bytes);
                    if (komunikat.GetOp() == "000100")
                    {
                        min = Convert.ToInt32(komunikat.GetLiczba(), 2);
                    }
                    break;
                }
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    komunikat.Resetruj();
                    komunikat.ustaw(bytes);
                    if (komunikat.GetOp() == "000101")
                    {
                        max = Convert.ToInt32(komunikat.GetLiczba(), 2);
                    }
                    break;
                }

                //Rozpoczecie gry
                Console.WriteLine("\nPrzedzial, w kotrym znajduje sie szukana liczba: " + min + "-" + max);
                Console.WriteLine("Zaczynamy gre! Podawaj kolejne liczby az nie trafisz. Powodzenia!");
                int zgadywana;

                while (true)
                {
                    komunikat.Resetruj();
                    do
                    {
                        temp = Console.ReadLine();
                        zgadywana = Convert.ToInt32(temp);
                        if (zgadywana > max || zgadywana < min)
                        {
                            Console.WriteLine("Podaj liczbe z przedzialu!");
                        }
                    } while (zgadywana > max || zgadywana < min);
                    Console.WriteLine("Oczekiwanie na przeciwnika...");

                    komunikat.SetLiczba(Convert.ToString(zgadywana, 2).PadLeft(8, '0'));
                    komunikat.SetOp("000110");//Zgadywana liczba
                    komunikat.GetBitArray().CopyTo(bytes, 0);
                    stream.Write(bytes, 0, bytes.Length);

                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        komunikat.Resetruj();
                        komunikat.ustaw(bytes);
                        if (komunikat.GetOp() == "000111")
                        {
                            switch (komunikat.GetOdp())
                            {
                                case "001":
                                    {
                                        Console.WriteLine("Wygrales!");
                                        stream.Close();
                                        client.Close();
                                        Console.ReadKey();
                                        Environment.Exit(0);
                                        break;
                                    }
                                case "010":
                                    {
                                        Console.WriteLine("Podana liczba jest za duza!");
                                        break;
                                    }
                                case "011":
                                    {
                                        Console.WriteLine("Podana liczba jest za mala!");
                                        break;
                                    }
                                case "100":
                                    {
                                        Console.WriteLine("Twoj przeciwnik wygrywa!");
                                        stream.Close();
                                        client.Close();
                                        Console.ReadKey();
                                        Environment.Exit(0);
                                        break;
                                    }
                                case "101":
                                    {
                                        Console.WriteLine("Remis!");
                                        stream.Close();
                                        client.Close();
                                        Console.ReadKey();
                                        Environment.Exit(0);
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                }



            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Nacisnij enter aby kontynuowac...");
            Console.Read();


        }
    }
}
