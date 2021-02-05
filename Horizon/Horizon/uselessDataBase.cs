
using SkiaSharp;

namespace Horizon
{

    static class uselessDataBase
    {
        static public void setUselessData(Planet p)
        {
            switch (p.name)
            {
                case "moon":
                    moon(p);
                    break;
                case "sun":
                    sun(p);
                    break;
                case "earth":
                    earth(p);
                    break;
                case "mercury":
                    mercury(p);
                    break;
                case "venus":
                    venus(p);
                    break;
                case "mars":
                    mars(p);
                    break;
                case "jupiter":
                    jupiter(p);
                    break;
                case "saturn":
                    saturn(p);
                    break;
                case "uranus":
                    uranus(p);
                    break;
                case "neptune":
                    neptune(p);
                    break;
                default: break;
            }
        }

        static private void moon(Planet p)
        {
            p.setColor(SKColors.Gray);
            p.Size = 70;
            p.printSize = 1;
            p.orbitRateo = 1;

            p.planetData.Add(3476.2 / 2);
            p.planetData.Add(37930000);
            p.planetData.Add(2.1958e19);
            p.planetData.Add(7.342e22);
            p.planetData.Add(3.3462e3);
            p.planetData.Add(1.622);
            p.planetData.Add(27.3208 / 24); //giorni 
            p.planetData.Add(27.3208);      //ore
            p.planetData.Add(0);
            p.planetData.Add(-233);
            p.planetData.Add(123);
        }
        static private void sun(Planet p)
        {
            p.setColor(SKColors.Yellow);
            p.printSize = 10;
            p.Size = 70;
            p.orbitRateo = 0;

            p.planetData.Add(1.391e9 / 2);
            p.planetData.Add(6.0877e18);
            p.planetData.Add(1.4122e27);
            p.planetData.Add(1.9891e30);
            p.planetData.Add(1.408e3);
            p.planetData.Add(274);
            p.planetData.Add(226000000);    //anni per fare il giro della galassia
            p.planetData.Add(27);           //anche il sole gira su se stesso
            p.planetData.Add(0);
            p.planetData.Add(5500);
            p.planetData.Add(5500);
        }
        static private void earth(Planet p)
        {
            p.setColor(SKColors.Aquamarine);
            p.printSize = 3;
            p.Size = 70;
            p.orbitRateo = 1;

            p.planetData.Add(12756.274 / 2);
            p.planetData.Add(5.094953216e14);
            p.planetData.Add(1.08321e21);
            p.planetData.Add(5.9726e24);
            p.planetData.Add(5.514e3);
            p.planetData.Add(9.78);
            p.planetData.Add(356.25); //giorni
            p.planetData.Add(23.9345);  //ore
            p.planetData.Add(1);
            p.planetData.Add(-89);
            p.planetData.Add(57);
        }
        static private void mercury(Planet p)
        {
            p.setColor(SKColors.Gray);
            p.printSize = 2;
            p.Size = 70;
            p.orbitRateo = 4.5;

            p.planetData.Add(4879.4 / 2);
            p.planetData.Add(7.5e13);
            p.planetData.Add(6.083e19);
            p.planetData.Add(3.3011e23);
            p.planetData.Add(5.427e3);
            p.planetData.Add(3.7);
            p.planetData.Add(87.969);
            p.planetData.Add(58.65);
            p.planetData.Add(0);
            p.planetData.Add(-183);
            p.planetData.Add(452);
        }
        static private void venus(Planet p)
        {
            p.setColor(SKColors.Orange);
            p.printSize = 3;
            p.Size = 70;
            p.orbitRateo = 1.6252781486426346239430351579884;

            p.planetData.Add(12103.6 / 2);
            p.planetData.Add(4.6e14);
            p.planetData.Add(9.2843e20);
            p.planetData.Add(4.8675e24);
            p.planetData.Add(5.243e3);
            p.planetData.Add(8.87);
            p.planetData.Add(224.701);
            p.planetData.Add(243.69 * 24);
            p.planetData.Add(0);
            p.planetData.Add(380);
            p.planetData.Add(494);
        }
        static private void mars(Planet p)
        {
            p.setColor(SKColors.Crimson);
            p.printSize = 2;
            p.Size = 70;
            p.orbitRateo = 0.53158660844250363901018922852984;

            p.planetData.Add(6804.9 / 2);
            p.planetData.Add(1.448e14);
            p.planetData.Add(1.6318e20);
            p.planetData.Add(6.4185e23);
            p.planetData.Add(3.934);
            p.planetData.Add(3.69);
            p.planetData.Add(686.96);
            p.planetData.Add(1.025957 * 24);
            p.planetData.Add(2);
            p.planetData.Add(-140);
            p.planetData.Add(20);
        }
        static private void jupiter(Planet p)
        {
            p.setColor(SKColors.Brown);
            p.printSize = 8;
            p.Size = 70;
            p.orbitRateo = 0.08432232740706534287693373354883;

            p.planetData.Add(142984 / 2);
            p.planetData.Add(6.1418738571e10);
            p.planetData.Add(1.43128e24);
            p.planetData.Add(1.89819e27);
            p.planetData.Add(1.326e3);
            p.planetData.Add(23.12);
            p.planetData.Add(4333.2867);
            p.planetData.Add(0.413538021 * 24);
            p.planetData.Add(79);
            p.planetData.Add(-163);
            p.planetData.Add(-88);
        }
        static private void saturn(Planet p)
        {
            p.setColor(SKColors.Ivory);
            p.printSize = 10;
            p.Size = 80;
            p.orbitRateo = 0.0339815762538382804503582395087;

            p.planetData.Add(120536 / 2);
            p.planetData.Add(4.26e16);
            p.planetData.Add(8.27e23);
            p.planetData.Add(5.6834e26);
            p.planetData.Add(0.687e3);
            p.planetData.Add(8.96);
            p.planetData.Add(29 * 356.4);
            p.planetData.Add(10.5);
            p.planetData.Add(62);
            p.planetData.Add(-191);
            p.planetData.Add(-98);
        }
        static private void uranus(Planet p)
        {
            p.setColor(new SKColor(122, 181, 207));
            p.printSize = 8;
            p.Size = 80;
            p.orbitRateo = 0.01193893229592337114649056850502;

            p.planetData.Add(51118 / 2);
            p.planetData.Add(8.1156e9);
            p.planetData.Add(6.833e10);
            p.planetData.Add(86.813e24);
            p.planetData.Add(1.271e3);
            p.planetData.Add(8.69);
            p.planetData.Add(84.011 * 356.4);
            p.planetData.Add(0.71833 * 24);
            p.planetData.Add(27);
            p.planetData.Add(-214);
            p.planetData.Add(-196);
        }
        static private void neptune(Planet p)
        {
            p.setColor(SKColors.DarkOrchid);
            p.printSize = 5;
            p.Size = 70;
            p.orbitRateo = 0.00610702341137123745819397993311;

            p.planetData.Add(49528 / 2);
            p.planetData.Add(7.619e15);
            p.planetData.Add(6.254e22);
            p.planetData.Add(1.0243e26);
            p.planetData.Add(1638);
            p.planetData.Add(11.15);
            p.planetData.Add(164.88); //giorni
            p.planetData.Add(16.11); //ore
            p.planetData.Add(14);
            p.planetData.Add(-223);
            p.planetData.Add(-216);
        }

    }
}