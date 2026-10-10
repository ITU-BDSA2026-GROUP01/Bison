module {:extern "Bison.Razor.Models"}ObservationFilter {

  class {:extern "Taxon"} Taxon {
    function {:extern}  {:axiom} IsSubTaxon(ancestor: Taxon): bool
  }

  class {:extern "Observation"} Observation {
    function {:extern} {:axiom} GetTaxon(): Taxon
  }

  //An observation matches a taxon if its taxon is the root itself or a subtaxon of it 
  predicate Matches(taxon: Taxon, obs: Observation)
  {
    var t := obs.GetTaxon();
    t == taxon || t.IsSubTaxon(taxon)
  }

  function FilterBy(root: Taxon, obs: seq<Observation>): seq<Observation>
    ensures forall o :: o in FilterBy(root, obs) ==> o in obs && Matches(root, o) //checks everything returnd came from input and matches the root
    ensures forall o :: o in obs && Matches(root, o) ==> o in FilterBy(root, obs) // checks every matching input observation is returned
  {
    if |obs| == 0 then []
    else if Matches(root, obs[0]) then [obs[0]] + FilterBy(root, obs[1..])
    else FilterBy(root, obs[1..])
  }
}

module FilterWrapper{
  import opened ObservationFilter

  method FilterWrapped(root: Taxon, obs: seq<Observation>) returns (filtered: seq<Observation>)
    ensures filtered == FilterBy(root, obs)
  {
    filtered := FilterBy(root, obs);
  }
}