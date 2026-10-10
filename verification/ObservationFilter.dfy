class {:extern} Taxon {
  function {:extern} isSubTaxon(ancestor: Taxon): bool
}

class {:extern} Observation {
  function {:extern} getTaxon(): Taxon
}
