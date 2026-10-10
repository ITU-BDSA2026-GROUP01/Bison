datatype Observation = Observation(taxonId: int)

method FilterByTaxon(taxonId: int, observations: seq<Observation>) returns (filtered: seq<Observation>)
{
  filtered := [];

  for i := 0 to |observations|
  {
    if observations[i].taxonId == taxonId {
      filtered := filtered + [observations[i]];
    }
  }
}
