public class TreeBuilder : ITreeBuilder
{
    public List<LicnostTreeDto> BuildTrees(List<LicnostFlatDto> flatList)
    {
        if (flatList == null || flatList.Count == 0)
            return new List<LicnostTreeDto>();

        var dict = flatList.ToDictionary(l => l.ID);

        var rootCandidates = flatList.Where(l =>
            l.DecaID.Count > 0 &&
            (
                l.RoditeljiID.Count == 0 ||
                !l.RoditeljiID.Any(pid => dict.ContainsKey(pid))
            )
        ).ToList();

        var result = new List<LicnostTreeDto>();

        foreach (var root in rootCandidates)
        {
            var visited = new HashSet<Guid>();
            var tree = BuildNode(root.ID, dict, visited);
            if (tree != null)
                result.Add(tree);
        }

        return result;
    }

    private LicnostTreeDto BuildNode(
        Guid id,
        Dictionary<Guid, LicnostFlatDto> dict,
        HashSet<Guid> visited
    )
    {
        if (!dict.TryGetValue(id, out var flat))
            return null;

        if (!visited.Add(id))
            return null; // prevent cycles

        var node = MapFlatToTree(flat);

        foreach (var childId in flat.DecaID)
        {
            if (!dict.ContainsKey(childId))
                continue;

            var child = BuildNode(childId, dict, visited);
            if (child != null)
                node.Deca.Add(child);
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
            Deca = new List<LicnostTreeDto>()
        };
    }
}
