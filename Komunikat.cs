using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klient
{
    class Komunikat
    {

        private
            String op = "000000", id = "000", odp = "000", liczba = "00000000";

        public void ustaw(Byte[] s)
        {
            BitArray tempBitArray = new BitArray(s);
            String temp = "";
            op = id = odp = liczba = "";
            int j = 0;
            for (int x = 7; x >= 0; x--)
            {
                temp += tempBitArray[x] ? '1' : '0';
            }
            for (int x = 15; x >= 8; x--)
            {
                temp += tempBitArray[x] ? '1' : '0';
            }
            for (int x = 23; x >= 16; x--)
            {
                temp += tempBitArray[x] ? '1' : '0';
            }
            int i = 0;
            while (i < 6)
            {
                op += temp[i];
                i++;
            }
            while (i < 9)
            {
                odp += temp[i];
                i++;
            }
            while (i < 12)
            {
                id += temp[i];
                i++;
            }
            while (i < 20)
            {
                liczba += temp[i];
                i++;
            }
        }

        public String GetOp()
        {
            return op;
        }
        public void SetOp(String op)
        {
            this.op = op;
        }

        public String GetId()
        {
            return id;
        }

        public void SetId(String id)
        {
            this.id = id;
        }
        public String GetOdp()
        {
            return odp;
        }

        public void SetOdp(String odp)
        {
            this.odp = odp;
        }
        public String GetLiczba()
        {
            return liczba;
        }
        public void SetLiczba(String liczba)
        {
            this.liczba = liczba;
        }

        public String GetMsg()
        {
            return op + odp + id + liczba + "0000";
        }

        public BitArray GetBitArray()
        {

            String temp = GetMsg();
            BitArray tempBitArray = new BitArray(24);
            int j = 0;
            for (int i = 7; i >= 0; i--)
            {
                if (temp[j] == '1')
                    tempBitArray[i] = true;
                else
                    tempBitArray[i] = false;
                j++;
            }
            for (int i = 15; i >= 8; i--)
            {
                if (temp[j] == '1')
                    tempBitArray[i] = true;
                else
                    tempBitArray[i] = false;
                j++;
            }
            for (int i = 23; i >= 16; i--)
            {
                if (temp[j] == '1')
                    tempBitArray[i] = true;
                else
                    tempBitArray[i] = false;
                j++;
            }
            return tempBitArray;
        }

        public void Resetruj()
        {
            op = "000000";
            odp = "000";
            liczba = "00000000";
        }
    }
}
