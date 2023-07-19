using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.Timeline;

/// <summary>
/// If a usable entity implements this interface, its use action will not be recorded.
/// Use this if the entity implements its own action on use (weapon pickup, etc)
/// </summary>
public interface IUseNotCanon
{

}
