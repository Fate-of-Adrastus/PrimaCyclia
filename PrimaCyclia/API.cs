//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace TrueSalt;

using AtomTypes = class_175;

public static class API
{
    public static string SalinizationPermission = "TrueSalt:salinization";
    public static string ConcentrationPermission = "TrueSalt:concentration";
    public static string DilutionPermission = "TrueSalt:dilution";
	public static string PrecipitationPermission = "TrueSalt:precipitation";
	public static string WheelPermission = "TrueSalt:minasawheel";

	public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

	#region atomtype getters
	public static AtomType tinAtomType => AtomTypes.field_1683;
	public static AtomType ironAtomType => AtomTypes.field_1684;
	public static AtomType copperAtomType => AtomTypes.field_1682;
	public static AtomType silverAtomType => AtomTypes.field_1685;
	public static AtomType goldAtomType => AtomTypes.field_1686;
    public static AtomType quintAtomType => AtomTypes.field_1690;
	public static AtomType[] singleSalinityAtoms => new AtomType[8] {
		AtomTypes.field_1676, // air
		AtomTypes.field_1677, // earth
		AtomTypes.field_1678, // fire
		AtomTypes.field_1679, // water
		AtomTypes.field_1687, // vitae
		AtomTypes.field_1688, // mors
		AtomTypes.field_1680, // quicksilver
		AtomTypes.field_1681, // lead
	};

	private static string ToString(AtomType A) => A.field_2284;
	
	private static string ruleToString<T>(AtomType hi, T lo)
	{
		if (typeof(T) == typeof(AtomType))
		{
			return ToString(hi) + " => " + ToString((AtomType)(object)lo);
		}
		else if (typeof(T) == typeof(Pair<AtomType, AtomType>))
		{
			return ToString(hi) + " => ( " + ToString(((Pair<AtomType, AtomType>)(object)lo).Left) + ", " + ToString(((Pair<AtomType, AtomType>)(object)lo).Right) + " )";
		}
		return "";
	}

	#endregion

	#region ruleDictionaries and methods
	public static Dictionary<AtomType, float> salinizeDict = new();

    public static Dictionary<float, AtomType> saltAtoms = new();
	public static Dictionary<AtomType, float> saltAtomsInverse = new();
	public static void addSaltAtom(AtomType input, float salinity)
	{
		
		if (saltAtoms.ContainsKey(salinity) || saltAtoms.ContainsValue(input))
		{
			Logger.Log("[TrueSalt] ERROR: Cannot add two salt atoms of the same salinity");
			throw new Exception("Adding "+ToString(input)+" with salinity "+salinity);
		}

		saltAtoms[salinity] = input;
		saltAtomsInverse[input] = salinity;

	}
    public static void addSalinizationRule(AtomType input, float salinity) {addTRule("rejection", input, salinity, salinizeDict, new List<AtomType>{});}

	public static AtomType salinizeAtom(AtomType input)
	{
		
		float atomSalinity = 1;
		
		if (salinizeDict.ContainsKey(input)) {atomSalinity  = salinizeDict[input];}

		AtomType output = input;

		foreach (var kvp in saltAtoms.OrderBy(kvp => kvp.Key))
		{
			if (kvp.Key > atomSalinity) break;
			output = kvp.Value;
		}

		return output;
	}

	private static bool TEquality<T>(T A, T B)
	{
		if (typeof(T) == typeof(AtomType))
		{
			return (AtomType)(object)A == (AtomType)(object)B;
		}
		else if (typeof(T) == typeof(Pair<AtomType, AtomType>))
		{
			return (Pair<AtomType, AtomType>)(object)A == (Pair<AtomType, AtomType>)(object)B;
		}
		return false;
	}
	private static void addTRule<T>(string Tname, AtomType input, T output, Dictionary<AtomType, T> dict, List<AtomType> forbiddenInputs)
	{
		string TNAME = Tname.First().ToString().ToUpper() + Tname.Substring(1);
		//check if rule is forbidden
		if (forbiddenInputs.Contains(input))
		{
			Logger.Log("[TrueSalt] ERROR: A " + Tname + " rule for " + ToString(input) + " is not permitted.");
			throw new Exception("add" + TNAME + "Rule: Cannot add rule '" + ruleToString(input, output) + "'.");
		}
		//try to add rule
		bool flag = dict.ContainsKey(input);
		if (flag && !TEquality(dict[input], output))
		{
			//throw an error
			string msg = "[TrueSalt] ERROR: Preparing debug dump.";
			msg += "\n  Current list of " + TNAME + " Rules:";
			foreach (var kvp in dict) msg += "\n\t" + ruleToString(kvp.Key, kvp.Value);
			msg += "\n\n  AtomType '" + ToString(input) + "' already has a " + Tname + " rule: '" + ruleToString(input, dict[input]) + "'.";
			Logger.Log(msg);
			throw new Exception("add" + TNAME + "Rule: Cannot add rule '" + ruleToString(input, output) + "'.");
		}
		else if (!flag)
		{
			dict.Add(input, output);
		}
	}

	#endregion
}