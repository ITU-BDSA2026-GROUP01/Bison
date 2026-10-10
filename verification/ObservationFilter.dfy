class {:extern} Taxon {
  function {:extern} getTaxonId(): string
  function {:extern} isSubTaxon(ancestor: Taxon): bool
}

class {:extern} Observation {
  function {:extern} getTaxon(): Taxon
}

function FilterBy(root: Taxon, obs: seq<Observation>): seq<Observation> decreases |obs|
{
  if |obs| == 0 then [] // if there are no observations, return an empty sequence
  else if obs[0].getTaxon().getTaxonId() == root.getTaxonId() || obs[0].getTaxon().isSubTaxon(root) // if the taxon of the first observation is equal to the root or is a sub-taxon of the root, include it in the filtered sequence and continue filtering the rest
  then [obs[0]] + FilterBy(root, obs[1..])
  else FilterBy(root, obs[1..]) // if the taxon of the first observation is not equal to the root and is not a sub-taxon of the root, skip it and continue filtering the rest
}