using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


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

namespace serwer
{
    class Program
    {
        static void Main()
        {
            TcpListener server = null;
            try
            {
                int L1 = 0, L2 = 0;
                bool wygrana1 = false, wygrana2 = false;
                string hostName = Dns.GetHostName(); // pobieranie nazwy 
                Console.WriteLine("Nazwa serwera: " + hostName);

                // uzyskanie adresu IP 
                string ip = Dns.GetHostByName(hostName).AddressList[0].ToString();
                Console.WriteLine("Adres IP:" + ip);

                int port = 51997;
                server = new TcpListener(IPAddress.Any, port);

                server.Start();

                Byte[] bytes1 = new Byte[3];
                Byte[] bytes2 = new Byte[3];

                const String id1 = "001", id2 = "010";
                Komunikat komunikat1 = new Komunikat(), komunikat2 = new Komunikat();

                {
                    Console.WriteLine("Oczekiwanie na polaczenie obu klientow... ");


                    TcpClient client = server.AcceptTcpClient();

                    NetworkStream stream1 = client.GetStream();


                    //nadanie identyfikatora klientowi 1
                    int i;
                    while ((i = stream1.Read(bytes1, 0, bytes1.Length)) != 0)
                    {


                        komunikat1.ustaw(bytes1);
                        if (komunikat1.GetOp() == "000001")
                        {
                            Console.WriteLine("Klient pierwszy polaczony! Nadano identyfikator " + Convert.ToInt32(id1, 2));
                            komunikat1.SetOp("000010");
                            komunikat1.SetId(id1);
                            komunikat1.GetBitArray().CopyTo(bytes1, 0);
                            stream1.Write(bytes1, 0, bytes1.Length);
                            break;
                        }
                    }


                    TcpClient client2 = server.AcceptTcpClient();

                    NetworkStream stream2 = client2.GetStream();

                    //nadanie identyfikatora klientowi 2
                    while ((i = stream2.Read(bytes2, 0, bytes2.Length)) != 0)
                    {
                        komunikat2.ustaw(bytes2);
                        if (komunikat2.GetOp() == "000001")
                        {
                            Console.WriteLine("Klient pierwszy polaczony! Nadano identyfikator " + Convert.ToInt32(id2, 2));
                            komunikat2.SetOp("000010");
                            komunikat2.SetId(id2);
                            komunikat2.GetBitArray().CopyTo(bytes2, 0);
                            stream2.Write(bytes2, 0, bytes2.Length);
                            break;
                        }
                    }

                    //odzczyt liczby przeslanej przez 1
                    while ((i = stream1.Read(bytes1, 0, bytes1.Length)) != 0)
                    {
                        komunikat1.ustaw(bytes1);
                        if (komunikat1.GetOp() == "000011")
                        {
                            L1 = Convert.ToInt32(komunikat1.GetLiczba(), 2);
                            Console.WriteLine(komunikat1.GetId() + " przesyla liczbe: " + Convert.ToString(L1), 2);
                            break;
                        }

                    }

                    //odzczyt liczby przeslanej przez 2
                    while ((i = stream2.Read(bytes2, 0, bytes2.Length)) != 0)
                    {
                        komunikat2.ustaw(bytes2);
                        if (komunikat2.GetOp() == "000011")
                        {
                            L2 = Convert.ToInt32(komunikat2.GetLiczba(), 2);
                            Console.WriteLine(komunikat2.GetId() + " przesyla liczbe: " + Convert.ToString(L2), 2);
                            break;
                        }
                    }


                    //Wyznaczanie przedzialu i wylosowanie liczby
                    int min, max;
                    min = L1 - L2;
                    if (min < 0) min = 0;
                    max = L1 + L2;
                    if (max > 200) max = 200;
                    Random random = new Random();
                    int zgadywana = random.Next(min, max);
                    Console.WriteLine("Wylosowano przedzial: <" + min + "," + max + ">\nZgadywna liczba to: " + zgadywana);

                    //przeslanie przedzialu klientowi 1
                    {
                        //min
                        komunikat1.Resetuj();
                        komunikat1.SetOp("000100");
                        //komunikat1.SetId(id1);
                        komunikat1.SetLiczba(Convert.ToString(min, 2).PadLeft(8, '0'));
                        komunikat1.GetBitArray().CopyTo(bytes1, 0);
                        stream1.Write(bytes1, 0, bytes1.Length);
                        //max
                        komunikat1.Resetuj();
                        komunikat1.SetOp("000101");
                        //komunikat1.SetId(id1);
                        komunikat1.SetLiczba(Convert.ToString(max, 2).PadLeft(8, '0'));
                        komunikat1.GetBitArray().CopyTo(bytes1, 0);
                        stream1.Write(bytes1, 0, bytes1.Length);
                    }

                    //przeslanie przedzialu 2

                    {
                        //min
                        komunikat2.Resetuj();
                        komunikat2.SetOp("000100");
                        //komunikat2.SetId(id2);
                        komunikat2.SetLiczba(Convert.ToString(min, 2).PadLeft(8, '0'));
                        komunikat2.GetBitArray().CopyTo(bytes2, 0);
                        stream2.Write(bytes2, 0, bytes2.Length);
                        //max
                        komunikat2.Resetuj();
                        komunikat2.SetOp("000101");
                        //komunikat2.SetId(id2);
                        komunikat2.SetLiczba(Convert.ToString(max, 2).PadLeft(8, '0'));
                        komunikat2.GetBitArray().CopyTo(bytes2, 0);
                        stream2.Write(bytes2, 0, bytes2.Length);
                    }

                    while (true)
                    {
                        while ((i = stream1.Read(bytes1, 0, bytes1.Length)) != 0)
                        {
                            komunikat1.ustaw(bytes1);
                            if (komunikat1.GetOp() == "000110")
                            {
                                //klient przesyla zgadywan liczbe
                                if (Convert.ToInt32(komunikat1.GetLiczba(), 2) == zgadywana)
                                {
                                    wygrana1 = true;
                                }
                                else if (Convert.ToInt32(komunikat1.GetLiczba(), 2) > zgadywana)
                                {
                                    komunikat1.Resetuj();
                                    komunikat1.SetOp("000111"); //odpowiedz serwera
                                    //komunikat1.SetId(id1);
                                    komunikat1.SetOdp("010"); //za duza
                                    komunikat1.GetBitArray().CopyTo(bytes1, 0);
                                    stream1.Write(bytes1, 0, bytes1.Length);
                                }
                                else if (Convert.ToInt32(komunikat1.GetLiczba(), 2) < zgadywana)
                                {
                                    komunikat1.Resetuj();
                                    komunikat1.SetOp("000111");
                                    //komunikat1.SetId(id1);
                                    komunikat1.SetOdp("011");//za mala
                                    komunikat1.GetBitArray().CopyTo(bytes1, 0);
                                    stream1.Write(bytes1, 0, bytes1.Length);
                                }
                                break;
                            }
                        }
                        while ((i = stream2.Read(bytes2, 0, bytes2.Length)) != 0)
                        {
                            komunikat2.ustaw(bytes2);
                            if (komunikat2.GetOp() == "000110")
                            {
                                //klient przesyla zgadywan liczbe

                                if (Convert.ToInt32(komunikat2.GetLiczba(), 2) == zgadywana)
                                {
                                    wygrana2 = true;
                                }
                                else if (Convert.ToInt32(komunikat2.GetLiczba(), 2) > zgadywana)
                                {
                                    komunikat2.Resetuj();
                                    komunikat2.SetOp("000111"); //odpowiedz serwera
                                    //komunikat2.SetId(id2);
                                    komunikat2.SetOdp("010"); //za duza
                                    komunikat2.GetBitArray().CopyTo(bytes2, 0);
                                    stream2.Write(bytes2, 0, bytes2.Length);
                                }
                                else if (Convert.ToInt32(komunikat2.GetLiczba(), 2) < zgadywana)
                                {
                                    komunikat2.Resetuj();
                                    komunikat2.SetOp("000111");
                                    //komunikat2.SetId(id2);
                                    komunikat2.SetOdp("011");//za mala
                                    komunikat2.GetBitArray().CopyTo(bytes2, 0);
                                    stream2.Write(bytes2, 0, bytes2.Length);
                                }
                                break;
                            }
                        }
                        if (wygrana1 && wygrana2)
                        {
                            komunikat1.Resetuj();
                            komunikat2.Resetuj();
                            komunikat1.SetOp("000111"); //odpowiedz serwera
                           //komunikat1.SetId(id1);
                            komunikat1.SetOdp("101"); //remis
                            komunikat1.GetBitArray().CopyTo(bytes1, 0);
                            stream1.Write(bytes1, 0, bytes1.Length);

                            komunikat2.SetOp("000111");
                            //komunikat2.SetId(id2);
                            komunikat2.SetOdp("101");//remis
                            komunikat2.GetBitArray().CopyTo(bytes2, 0);
                            stream2.Write(bytes2, 0, bytes2.Length);
                            break;
                        }
                        if (wygrana1)
                        {
                            komunikat1.Resetuj();
                            komunikat2.Resetuj();
                            komunikat1.SetOp("000111"); //odpowiedz serwera
                            //komunikat1.SetId(id1);
                            komunikat1.SetOdp("001"); //wygrana
                            komunikat1.GetBitArray().CopyTo(bytes1, 0);
                            stream1.Write(bytes1, 0, bytes1.Length);

                            komunikat2.SetOp("000111");
                            //komunikat2.SetId(id2);
                            komunikat2.SetOdp("100");//przegrana
                            komunikat2.GetBitArray().CopyTo(bytes2, 0);
                            stream2.Write(bytes2, 0, bytes2.Length);
                            break;
                        }
                        if (wygrana2)
                        {
                            komunikat1.Resetuj();
                            komunikat2.Resetuj();
                            komunikat1.SetOp("000111"); //odpowiedz serwera
                            //komunikat1.SetId(id1);
                            komunikat1.SetOdp("100"); //przegrana
                            komunikat1.GetBitArray().CopyTo(bytes1, 0);
                            stream1.Write(bytes1, 0, bytes1.Length);

                            komunikat2.SetOp("000111");
                            //komunikat2.SetId(id2);
                            komunikat2.SetOdp("001");//wygrana
                            komunikat2.GetBitArray().CopyTo(bytes2, 0);
                            stream2.Write(bytes2, 0, bytes2.Length);
                            break;
                        }


                    }

                }

            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                Console.WriteLine("\nNacisnij enter aby wyjsc...");
                Console.Read();
                server.Stop();
            }

        }
    }
}
