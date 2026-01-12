public class TreeBuilder : ITreeBuilder
{
    public List<LicnostTreeDto> BuildTrees(List<LicnostFlatDto> flatList)
    {
        if (flatList == null || flatList.Count == 0)
            return new List<LicnostTreeDto>();

        var dict = flatList.ToDictionary(l => l.ID);

        // GLOBAL ownership tracker
        var assigned = new HashSet<Guid>();

        var rootCandidates = flatList
            .Where(l =>
                l.DecaID.Count > 0 &&
                (
                    l.RoditeljiID.Count == 0 ||
                    !l.RoditeljiID.Any(pid => dict.ContainsKey(pid))
                )
            )
            .ToList();

        var result = new List<LicnostTreeDto>();

        foreach (var root in rootCandidates)
        {
            if (assigned.Contains(root.ID))
                continue;

            var visited = new HashSet<Guid>();
            var tree = BuildNode(root.ID, dict, visited, assigned, null);

            if (tree != null)
                result.Add(tree);
        }

        return result;
    }

    private LicnostTreeDto BuildNode(
        Guid id,
        Dictionary<Guid, LicnostFlatDto> dict,
        HashSet<Guid> visited,
        HashSet<Guid> assigned,
        LicnostTreeDto? parent
    )
    {
        if (!dict.TryGetValue(id, out var flat))
            return null;

        // cycle protection (local)
        if (!visited.Add(id))
            return null;

        // GLOBAL duplicate protection
        if (!assigned.Add(id))
            return null;

        if(flat.RoditeljiID.Count == 2)//ima 2 roditelja znaci njegovi roditetlji imaju supruznike
        {
            foreach(var rId in flat.RoditeljiID)
            {
                if(parent!= null && rId != parent.ID && !parent.SupruzniciID.Contains(rId))
                {
                    parent.SupruzniciID.Add(rId);
                    break;
                }
            }
        }

        var node = MapFlatToTree(flat);

        foreach (var childId in flat.DecaID)
        {
            if (!dict.ContainsKey(childId))
                continue;

            var child = BuildNode(childId, dict, visited, assigned, node);
            if (child != null)
                node.Deca.Add(child);
        }
        if(node.SupruzniciID != null && node.SupruzniciID.Count > 0)
        {
            foreach(var sId in node.SupruzniciID)
            {
                var spouse = BuildNode(sId, dict, visited, assigned, null);
                node.Supruznici.Add(spouse);
            }
            
        }
        return node;
    }

    private LicnostTreeDto MapFlatToTree(LicnostFlatDto src)
    {
        return new LicnostTreeDto
        {
            ID = src.ID,
            Titula = src.Titula,
            Ime = src.Ime,
            Prezime = src.Prezime,
            GodinaRodjenja = src.GodinaRodjenja,
            GodinaRodjenjaPNE = src.GodinaRodjenjaPNE,
            GodinaSmrti = src.GodinaSmrti,
            GodinaSmrtiPNE = src.GodinaSmrtiPNE,
            Pol = src.Pol,
            MestoRodjenja = src.MestoRodjenja,
            Tekst = src.Tekst,
            Slika = src.Slika,
            Deca = new List<LicnostTreeDto>(),
            DecaID = src.DecaID,
            RoditeljiID = src.RoditeljiID,
            SupruzniciID = src.SupruzniciID
        };
    }
}
