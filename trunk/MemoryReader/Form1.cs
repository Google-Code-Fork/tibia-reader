#region using
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
#endregion

namespace MemoryReader
{
    public partial class Form1 : Form
    {
        #region declarations
        public static UInt32 Base = 0;
        public static Process Tibia = null;
        blist[] lst;
        //Character info
        UInt32 Pxor = 0x3B3ED0;
        UInt32 HpAdr = 0x549000;
        UInt32 XpAdr = 0x3B3EE0;
        UInt32 MpAdr = 0x3B3F24;
        UInt32 CpAdr = 0x580E94;
        UInt32 SlAdr = 0x3B3F10;
        UInt32 StAdr = 0x3B3F58;
        UInt32 MlAdr = 0x3B3ED4;
        UInt32 IdAdr = 0x580EA4;
        // Battle list
        UInt32 BlStart = 0x549008;
        UInt32 BlStep = 0xB0;
        public int MaxCreatures = 1300;
        public byte[] BattleListCache = new byte[40160];
        // Battle list offsets
        UInt32 OFFSET_CREATURE_ID = 0;
        UInt32 OFFSET_CREATURE_TYPE = 3;
        UInt32 OFFSET_CREATURE_NAME = 4;
        UInt32 OFFSET_CREATURE_Z = 36;
        UInt32 OFFSET_CREATURE_Y = 40;
        UInt32 OFFSET_CREATURE_X = 44;
        UInt32 OFFSET_CREATURE_IS_WALKING = 80;
        UInt32 OFFSET_CREATURE_DIRECTION = 84;
        UInt32 OFFSET_CREATURE_OUTFIT = 100;
        UInt32 OFFSET_CREATURE_OUTFIT_HEAD = 104;
        UInt32 OFFSET_CREATURE_OUTFIT_BODY = 108;
        UInt32 OFFSET_CREATURE_OUTFIT_LEGS = 112;
        UInt32 OFFSET_CREATURE_OUTFIT_FEET = 116;
        UInt32 OFFSET_CREATURE_OUTFIT_ADDON = 120;
        UInt32 OFFSET_CREATURE_LIGHT = 124;
        UInt32 OFFSET_CREATURE_LIGHT_COLOR = 128;
        UInt32 OFFSET_CREATURE_HP_BAR = 140;
        UInt32 OFFSET_CREATURE_WALK_SPEED = 144;
        UInt32 OFFSET_CREATURE_IS_VISIBLE = 148;
        UInt32 OFFSET_CREATURE_SKULL = 152;
        UInt32 OFFSET_CREATURE_PARTY = 156;
        UInt32 OFFSET_CREATURE_WARICON = 164;
        UInt32 OFFSET_CREATURE_ISBLOCKING = 168;
        UInt32 OFFSET_CREATURE_WITHINVIEW = 172;
        int[] offsets = new int[24] { 0, 3, 4, 36, 40, 44, 80, 84, 100, 104, 108, 112, 116, 120, 124, 128, 140, 144, 148, 152, 156, 164, 168, 172};

        [DllImport("kernel32.dll")]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

        #endregion

        #region readmem

        public static byte[] ReadBytes(IntPtr Handle, Int64 Address, uint BytesToRead)
        {
            IntPtr ptrBytesRead;
            // Declare a buffer, this is the no mans land in which the information travels to get from the memory address to our programs memory.
            byte[] buffer = new byte[BytesToRead];
            // Call to the windows function to get the information.
            ReadProcessMemory(Handle, new IntPtr(Address), buffer, BytesToRead, out ptrBytesRead);
            // The result of this function will be the contents of buffer. Any information which was stored at the memory address passed in, is now in the buffer.
            return buffer;
        }

        public static int ReadInt32(IntPtr Handle, long Address)
        {
            return BitConverter.ToInt32(ReadBytes(Handle, Address, 4), 0);
        }

        public static string ReadString(IntPtr Handle, long Address)
        {
            string temp3 = ASCIIEncoding.Default.GetString(ReadBytes(Handle, Address, 32));
            return temp3.Replace("\0", string.Empty);

        }

        public int hp() { return ReadInt32(Tibia.Handle, HpAdr + Base) ^ ReadInt32(Tibia.Handle, (Pxor + Base)); }
        public int xp() { return ReadInt32(Tibia.Handle, XpAdr + Base); }
        public int mp() { return ReadInt32(Tibia.Handle, MpAdr + Base) ^ ReadInt32(Tibia.Handle, (Pxor + Base)); }
        public int cp() { return (ReadInt32(Tibia.Handle, CpAdr + Base) ^ ReadInt32(Tibia.Handle, (Pxor + Base))) / 100; }
        public int id() { return ReadInt32(Tibia.Handle, IdAdr + Base); }
        public int ml() { return ReadInt32(Tibia.Handle, MlAdr + Base) ^ ReadInt32(Tibia.Handle, (Pxor + Base)); }
        public int sl() { return ReadInt32(Tibia.Handle, SlAdr + Base); }
        public string st() { int minb = ReadInt32(Tibia.Handle, StAdr + Base); int houa = minb / 60; int mina = minb % 60; return houa.ToString() + ":" + mina.ToString("D2"); }

        #endregion

        #region formelements

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            refreshclients();
            /* This will automatically select the first client in the list, remove it for release */
            selectclient();
            this.Text = "Tibia Reader - " + getname();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            reselectclient();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            updateinfo();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            refreshclients();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            readBlist();
            updateBlist();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            reselectclient();
            readBlist();
            updateBlist();
            updateinfo();
            this.Text = "Tibia Reader - " + getname();
        }

        #endregion

        #region myfuncts

        private void refreshclients()
        {
            Process[] TibiaProcess = Process.GetProcessesByName("Tibia");
            listBox1.Items.Clear();
            foreach (Process p in TibiaProcess)
            {
                listBox1.Items.Add("Process ID:" + Convert.ToString(p.Id));
            }
        }

        private void updateinfo()
        {
            listBox2.Items.Clear();
            listBox2.Items.Add("HP: " + Convert.ToString(hp()));
            listBox2.Items.Add("MP: " + Convert.ToString(mp()));
            listBox2.Items.Add("XP: " + Convert.ToString(xp()));
            listBox2.Items.Add("Cap: " + Convert.ToString(cp()));
            listBox2.Items.Add("ID: " + Convert.ToString(id()));
            listBox2.Items.Add("ML: " + Convert.ToString(ml()));
            listBox2.Items.Add("Soul: " + Convert.ToString(sl()));
            listBox2.Items.Add("Stamina: " + st());
        }

        private void selectclient()
        {
            Process[] TibiaProcess = Process.GetProcessesByName("Tibia");
            Tibia = TibiaProcess[0];
            Base = Convert.ToUInt32(Tibia.MainModule.BaseAddress.ToInt32());
        }

        private void reselectclient()
        {
            Process[] TibiaProcesses = Process.GetProcessesByName("Tibia");
            string[] strs = Convert.ToString(listBox1.SelectedItem).Split(':');
            foreach (Process pr in TibiaProcesses)
            {
                if (Convert.ToString(pr.Id) == strs[1])
                {
                    Tibia = pr;
                    Base = Convert.ToUInt32(Tibia.MainModule.BaseAddress.ToInt32());
                }
            }
        }

        private string getname()
        {
            // Battle list must be read before name can be grabbed!
            readBlist();
            foreach (blist item in lst)
            {
                if (item.cid == id())
                {
                    return item.name;
                }
            }
            return "No client found";
        }

        private void readBlist()
        {
            //ReadProcessMemory(Form1.Tibia.Handle, (BlStart - 160), BattleListCache, 40160, ptrBytesRead);
            int max = Convert.ToInt32(BlStep) * MaxCreatures;
            lst = new blist[250];
            UInt32 current = BlStart;
            for (int i = 0; i < 250; i++)
            {
                if (ReadInt32(Tibia.Handle, current + Base) != 0)
                {
                    int[] offsets = new int[24] { 0, 3, 4, 36, 40, 44, 80, 84, 100, 104, 108, 112, 116, 120, 124, 128, 140, 144, 148, 152, 156, 164, 168, 172 };
                    int b = 0;
                    lst[i].cid = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].type = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].name = ReadString(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].z = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].y = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].x = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].iswalking = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].direction = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].outfit = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].outfithead = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].outfitbody = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].outfitlegs = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].outfitfeet = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].outfitaddon = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].light = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].lightcolour = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].hpbar = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].walkspeed = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].unknown = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].skull = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].party = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].war = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                    lst[i].blocking = ReadInt32(Tibia.Handle, current + offsets[b] + Base);
                    lst[i].visible = ReadInt32(Tibia.Handle, current + offsets[b] + Base); b = b + 1;
                }
                current = current + BlStep;
            }
        }

        private void updateBlist()
        {
            dataGridView1.Rows.Clear();
            foreach (blist mob in lst)
            {
                if (mob.cid != 0)
                {
                    dataGridView1.Rows.Add(mob.cid, mob.name, mob.x + ", " + mob.y + ", " + mob.z, mob.hpbar, mob.visible);
                }
            }
        }

        #endregion

        #region structures

        public struct blist
        {
            public int cid;
            public int type;
            public string name;
            public int z;
            public int y;
            public int x;
            public int iswalking;
            public int direction;
            public int outfit;
            public int outfithead;
            public int outfitbody;
            public int outfitlegs;
            public int outfitfeet;
            public int outfitaddon;
            public int light;
            public int lightcolour;
            public int hpbar;
            public int walkspeed;
            public int unknown;
            public int skull;
            public int party;
            public int war;
            public int blocking;
            public int visible;            
        }

        #endregion

    }
}
