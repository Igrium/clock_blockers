using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Weapon;

public partial class Pistol : BaseFirearm
{
	public override string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	
}
