﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Horizon
{
    static class ConstellationsDB
    {

        public static Constellation[] getAll()
        {
            Constellation[] cs = {
                new Constellation( "Aquila", new int[]{7602, 7557, 7557, 7525, 7557, 7377, 7377, 7570, 7710, 7570, 7377, 7235, 7235, 7176, 7377, 7236} ),
                new Constellation( "Andromeda", new int[]{15, 165, 165, 337, 603, 337, 337, 269, 269, 226} ),
                new Constellation( "Sculptor", new int[]{8937, 280, 280, 8863, 8863, 8937} ),
                new Constellation( "Ara", new int[]{6743, 6510, 6510, 6285, 6285, 6229, 6229, 6500, 6500, 6462, 6462, 6461, 6461, 6743} ),
                new Constellation( "Libra", new int[]{5908, 5787, 5787, 5685, 5685, 5531, 5531, 5603, 5603, 5787} ),
                new Constellation( "Cetus", new int[]{649, 718, 509, 188, 188, 74, 188, 334, 334, 402, 402, 539, 539, 708, 708, 781, 781, 811, 811, 740, 740, 509, 681, 781, 681, 779, 779, 804, 804, 911, 911, 896, 896, 813, 813, 718, 718, 754, 754, 804} ),
                new Constellation( "Aries", new int[]{838, 617, 617, 553, 553, 546} ),
                new Constellation( "Scutum", new int[]{7063, 7066, 7066, 7119, 7119, 6930, 6930, 6973, 6973, 7063} ),
                new Constellation( "Pyxis", new int[]{3438, 3468, 3468, 3518} ),
                new Constellation( "Boötes", new int[]{5477, 5340, 5340, 5505, 5505, 5681, 5681, 5602, 5602, 5435, 5435, 5429, 5429, 5340, 5340, 5235, 5235, 5200} ),
                new Constellation( "Caelum", new int[]{1443, 1502, 1502, 1503} ),
                new Constellation( "Chamaeleon", new int[]{3318, 4174, 4174, 4674} ),
                new Constellation( "Cancer", new int[]{3475, 3449, 3449, 3262, 3449, 3461, 3461, 3249, 3461, 3572} ),
                new Constellation( "Capricornus", new int[]{7754, 7776, 7776, 8075, 8075, 8167, 8167, 8278, 8278, 8322, 8167, 8204, 8204, 8075, 7776, 7936, 8075, 7980} ),
                new Constellation( "Carina", new int[]{3685, 4037, 4037, 4199, 4199, 4200, 4200, 4337, 4337, 4257, 4257, 4114, 4114, 4050, 4050, 3699, 3457, 3307, /*3307, None,*/ 3659, 3699, 3659, 3457, /*None, 2451,*/ 3307, 3165} ),
                new Constellation( "Cassiopeia", new int[]{542, 403, 403, 264, 264, 168, 168, 21} ),
                new Constellation( "Centaurus", new int[]{5459, 5267, 5267, 5132, 5132, 5231, 5231, 5249, 5249, 5193, 5193, 5190, 5190, 5089, 5089, 5028, 5190, 5288, 5193, 5440, 5440, 5576, 5231, 4819, 4819, 4743, 4743, 4621, 4621, 4460, 4460, 4467} ),
                new Constellation( "Cepheus", new int[]{8465, 8694, 8694, 8238, 8238, 8162, 8162, 8465, 8694, 8974, 8974, 8238} ),
                new Constellation( "Coma Berenices", new int[]{4968, 4983, 4983, 4737} ),
                new Constellation( "Canes Venatici", new int[]{4785, 4915} ),
                new Constellation( "Auriga", new int[]{2095, 2088, 2088, 1708, 1708, 1612, 1612, 1577, 1791, 1577, 1791, 2095} ),
                new Constellation( "Columba", new int[]{2296, 2256, 2256, 2106, 2106, 2040, 2040, 2120, 2040, 1956, 1956, 1862} ),
                new Constellation( "Circinus", new int[]{5463, 5704, 5463, 5670} ),
                new Constellation( "Crater", new int[]{4287, 4343, 4343, 4405, 4405, 4382, 4382, 4287, 4382, 4402, 4402, 4468, 4468, 4567, 4567, 4514, 4514, 4405} ),
                new Constellation( "Corona Australis", new int[]{7021, 7129, 7129, 7152, 7152, 7226, 7226, 7254, 7254, 7259, 7259, 7242, 7242, 7188, 7188, 7122, 7021, 6942} ),
                new Constellation( "Corona Borealis", new int[]{5778, 5747, 5747, 5793, 5793, 5849, 5849, 5889, 5889, 5947, 5947, 5971} ),
                new Constellation( "Corvus", new int[]{4775, 4757, 4757, 4662, 4662, 4630, 4630, 4623, 4630, 4786, 4786, 4757} ),
                new Constellation( "Crux", new int[]{4763, 4730, 4853, 4656} ),
                new Constellation( "Cygnus", new int[]{7328, 7420, 7420, 7528, 7528, 7796, 7796, 7924, 7796, 7949, 7949, 8115, 8115, 8309, 7796, 7615, 7615, 7417} ),
                new Constellation( "Delphinus", new int[]{7852, 7882, 7882, 7906, 7906, 7948, 7948, 7928, 7928, 7882} ),
                new Constellation( "Dorado", new int[]{2015, 2102, 2102, 1922, 1922, 2015, 1922, 1465, 1465, 1338} ),
                new Constellation( "Draco", new int[]{6688, 6705, 6705, 6536, 6536, 6555, 6555, 6688, 6688, 7310, 7310, 7582, 7582, 7352, 7352, 6927, 6927, 6396, 6396, 6132, 6132, 5986, 5986, 5744, 5744, 5291, 5291, 4787, 4787, 4434} ),
                new Constellation( "Norma", new int[]{6024, 6072, 6072, 6115, 6115, 5962, 5962, 6072, 5962, 6024} ),
                new Constellation( "Eridanus", new int[]{472, 566, 566, 674, 674, 721, 721, 789, 789, 794, 794, 897, 897, 1008, 1008, 1189, 1189, 1195, 1195, 1347, 1347, 1393, 1393, 1464, 1464, 1173, 1173, 1088, 1088, 1003, 1003, 919, 919, 818, 818, 874, 874, 984, 984, 1084, 1084, 1136, 1136, 1463, 1463, 1520, 1520, 1560, 1560, 1666, 1666, 1679, 1679, 1481} ),
                new Constellation( "Sagitta", new int[]{7488, 7536, 7536, 7479, 7536, 7635, 7635, 7679} ),
                new Constellation( "Fornax", new int[]{841, 963} ),
                new Constellation( "Gemini", new int[]{2421, 2650, 2650, 2777, 2777, 2763, 2763, 2484, 2777, 2905, 2905, 2985, 2905, 2990, 2905, 2821, 2821, 2697, 2697, 2891, 2697, 2540, 2697, 2473, 2473, 2343, 2473, 2286, 2286, 2216, 2216, 2134} ),
                new Constellation( "Camelopardalis", new int[]{1035, 1204, 1204, 1542, 1035, 1148, 1148, 1542, 1148, 1686} ),
                new Constellation( "Canis Major", new int[]{2574, 2657, 2657, 2596, /*2596, None,*/ /*None, 2653,*/ 2653, 2693, 2693, 2749, 2749, 2827, 2618, 2646, 2646, 2693, 2646, 2583, 2583, 2429, 2429, 2414, 2429, 2294, /*2429, None,*/ 2618, 2538, 2282, 2618, 2596, 2574} ),
                new Constellation( "Ursa Major", new int[]{5191, 5054, 5054, 4905, 4905, 4660, 4660, 4301, 4301, 4295, 4295, 4554, 4554, 4660, 4554, 4518, 4518, 4335, 4335, 4033, 4335, 4069, 4295, 3894, 3894, 3775, 3775, 3594, 3775, 3569, 3894, 3888, 3888, 3323, 3323, 3757, 3757, 4301} ),
                new Constellation( "Grus", new int[]{8787, 8556, 8556, 8425, 8425, 8636, 8636, 8820, 8820, 8787, 8636, 8747, 8636, 8675, 8425, 8411, 8411, 8353} ),
                new Constellation( "Hercules", new int[]{6588, 6695, 6695, 6484, 6484, 6436, 6436, 6418, 6418, 6220, 6220, 6168, 6168, 6092, 6092, 5914, 6220, 6212, 6212, 6148, 6148, 6095, 6212, 6324, 6324, 6526, 6526, 6410, 6623, 6703, 6703, 6779, 6324, 6418, 6623, 6526} ),
                new Constellation( "Horologium", new int[]{1326, 802, 802, 934} ),
                new Constellation( "Hydra", new int[]{3454, 3418, 3418, 3410, 3410, 3482, 3482, 3492, 3492, 3454, 3492, 3547, 3547, 3665, 3665, 3787, 3787, 3759, 3759, 3748, 3748, 3903, 3903, 3994, 3994, 4094, 4094, 4232, 4232, 4450, 4450, 4552, 4552, 4958, 4958, 5020} ),
                new Constellation( "Hydrus", new int[]{98, 1208, 1208, 806, 806, 705, 705, 591} ),
                new Constellation( "Indus", new int[]{8140, 7869, 7869, 7986, 7986, 8140} ),
                new Constellation( "Lacerta", new int[]{8498, 8579, 8579, 8572, 8572, 8541, 8541, 8538, 8538, 8585, 8585, 8572} ),
                new Constellation( "Monoceros", new int[]{2227, 2356, 2356, 2714, 2714, 2298, 2298, 2174, 2714, 3188, 3188, 2970} ),
                new Constellation( "Lepus", new int[]{2155, 2085, 2085, 1998, 1998, 1865, 1865, 1702, 1865, 2035, 2035, 1983, 1983, 1829, 1829, 1654, 1865, 1829, 1702, 1756, 1702, 1705, 1654, 1702, 1705, 1696, 1756, 1757} ),
                new Constellation( "Leo", new int[]{4534, 4359, 4359, 3982, 3982, 3975, 3975, 4057, 4057, 4357, 4357, 4534, 4057, 4031, 4031, 3905, 3905, 3873, 4357, 4359} ),
                new Constellation( "Lupus", new int[]{5883, 5991, 5991, 5948, 5948, 5883, 5948, 5776, 5776, 5695, 5695, 5705, 5695, 5571, 5776, 5797, 5797, 5649, 5649, 5469, 5649, 5453, 5469, 5396, 5469, 5571} ),
                new Constellation( "Lynx", new int[]{3705, 3690, 3690, 3612, 3612, 3579, 3579, 3275, 3275, 2818, 2818, 2560, 2560, 2238} ),
                new Constellation( "Lyra", new int[]{7001, 7056, 7056, 7106, 7106, 7178, 7178, 7139, 7139, 7056} ),
                new Constellation( "Antlia", new int[]{4104, 3947} ),
                new Constellation( "Microscopium", new int[]{8135, 8039, 8039, 7965} ),
                new Constellation( "Musca", new int[]{4844, 4520, 4520, 4773, 4773, 4798, 4798, 4844} ),
                new Constellation( "Octans", new int[]{8254, 8630, 8630, 5339, 5339, 8254} ),
                new Constellation( "Apus", new int[]{5470, 6102, 6102, 6163} ),
                new Constellation( "Ophiuchus", new int[]{6556, 6603, 6378, 6603, 6556, 6299, 6299, 6075, 6075, 6175, 6175, 6378, 6378, 6519} ),
                new Constellation( "Orion", new int[]{1948, 1903, 1903, 1852, 2198, 2199, 2198, 2135, 2135, 2047, 2199, 2159, 2159, 2047, 2199, 2124, 2124, 2061, 2061, 1948, 1948, 2004, 2004, 1713, 1713, 1852, 1852, 1790, 1790, 1879, 1879, 2061, 1790, 1543, 1543, 1552, 1552, 1562, 1562, 1601, 1543, 1544, 1544, 1570, 2159, 2124} ),
                new Constellation( "Pavo", new int[]{7790, 8181, 8181, 7913, 7913, 7665, 7665, 7790, 7665, 7590, 7590, 6982, 6982, 7107, 7107, 7665, 7107, 7074, 7074, 6855, 6855, 6745, 6745, 7074, 6745, 6582} ),
                new Constellation( "Pegasus", new int[]{39, 8781, 8775, 8650, 8650, 8449, 8775, 8684, 8684, 8667, 8667, 8430, 8430, 8315, 8781, 8665, 8665, 8634, 8634, 8450, 8450, 8308, 15, 8775, 15, 39, 8775, 8781} ),
                new Constellation( "Pictor", new int[]{2550, 2042, 2042, 2020} ),
                new Constellation( "Perseus", new int[]{1131, 1203, 1203, 1228, 1228, 1220, 1220, 1122, 1122, 1017, 1017, 915, 915, 834, 1017, 936, 936, 921, 921, 840} ),
                new Constellation( "Equuleus", new int[]{8097, 8123, 8123, 8178, 8178, 8131, 8131, 8097} ),
                new Constellation( "Canis Minor", new int[]{2943, 2845} ),
                new Constellation( "Leo Minor", new int[]{4247, 4100, 4100, 3974, 3974, 3800, 3974, 4247} ),
                new Constellation( "Vulpecula", new int[]{7405, 7653} ),
                new Constellation( "Ursa Minor", new int[]{424, 6789, 6789, 6322, 6322, 5903, 5903, 6116, 6116, 5735, 5735, 5563, 5563, 5903} ),
                new Constellation( "Phoenix", new int[]{338, 322, 322, 100, 100, 338, 322, 440, 440, 555, 555, 322, 322, 429, 429, 100, 100, 99, 99, 25, 25, 100} ),
                new Constellation( "Pisces", new int[]{291, 360, 291, 383, 383, 360, 360, 437, 437, 510, 510, 595, 595, 549, 549, 489, 489, 434, 434, 294, 294, 221, 221, 80, 80, 9072, 9072, 8969, 8969, 8984, 8984, 8911, 8911, 8852, 8852, 8916, 8916, 8969} ),
                new Constellation( "Piscis Austrinus", new int[]{8728, 8628, 8628, 8386, 8386, 8326, 8326, 8447, 8447, 8576, 8576, 8720} ),
                new Constellation( "Volans", new int[]{3024, 2736, 2736, 3223, 3223, 3024, 3223, 2803, 3223, 3347, 3347, 3615, 3615, 3223} ),
                new Constellation( "Puppis", new int[]{3185, 3043, 3043, 2773, 2773, 2451, 2451, 2553, 2553, 2878, 2878, 3165, 3165, 3185} ),
                new Constellation( "Reticulum", new int[]{1336, 1355, 1355, 1247, 1247, 1175, 1175, 1336} ),
                new Constellation( "Sagittarius", new int[]{6859, 6913, 6832, 6879, 6879, 6746, 6746, 6616, 6746, 6859, 6859, 6879, 6879, 7194, 7194, 7039, 7039, 6859, 7039, 6913, 6913, 6812, 7194, 7234, 7234, 7121, 7121, 7039, 7121, 7150, 7150, 7217, 7217, 7304, 7304, 7340, 7234, 7431, 7431, 7650, 7650, 7623, 7623, 7581, 7581, 7348, 7581, 7343} ),
                new Constellation( "Scorpius", new int[]{6527, 6580, 6580, 6615, 6615, 6553, 6553, 6380, 6380, 6262, 6262, 6247, 6247, 6241, 6241, 6165, 6165, 6134, 6134, 5953, 6134, 5944, 6134, 5984} ),
                new Constellation( "Serpens", new int[]{5881, 5892, 5892, 5854, 5854, 5788, 5788, 5867, 5867, 5933, 5933, 5879, 5879, 5867, 7141, 6869, 6869, 6581, 6581, 6561, 6561, 6446} ),
                new Constellation( "Sextans", new int[]{4119, 3981} ),
                new Constellation( "Mensa", new int[]{1953, 1541} ),
                new Constellation( "Taurus", new int[]{1791, 1497, 1497, 1409, 1457, 1910, 1346, 1373, 1346, 1239, 1239, 1030, 1457, 1409, 1457, 1412, 1412, 1346, 1409, 1389, 1389, 1373, 1373, 1178} ),
                new Constellation( "Telescopium", new int[]{6905, 6897} ),
                new Constellation( "Tucana", new int[]{8502, 8848, 8848, 77, 8848, 126} ),
                new Constellation( "Triangulum", new int[]{664, 622, 622, 544, 544, 664} ),
                new Constellation( "Tra", new int[]{6217, 5671, 5671, 5897, 5897, 6217} ),
                new Constellation( "Aquarius", new int[]{8232, 8414, 8414, 8518, 8518, 8558, 8558, 8597, 8597, 8698, 8698, 8841, 8841, 8892, 8414, 8499, 8499, 8418, 8499, 8573, 8573, 8679, 8679, 8709, 8709, 8812, 7950, 8232} ),
                new Constellation( "Virgo", new int[]{4517, 4681, 4681, 4825, 4825, 5056, 5056, 5315, 5315, 5338, 5338, 5487, 5056, 5107, 5107, 5264, 5264, 5511, 5107, 4910, 4910, 4932, 4910, 4825} ),
                new Constellation( "Vela", new int[]{3207, 3447, 3447, 3485, 3485, 3734, 3734, 3940, 3940, 4216, 4216, 4167, 4167, 4023, 4023, 3786, 3786, 3634, 3634, 3207} )
            };
            return cs;
        }
    }
}