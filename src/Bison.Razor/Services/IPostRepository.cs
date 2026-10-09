using Bison.Razor.Models;
using Bison.Razor.Data;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Services;

public interface IPostRepository
{
    List<ObservationDTO> GetAllObservations(int page = 1);
    List<ObservationDTO> GetObservationsByAuthor(string author, int page = 1);

    /// <summary>
    /// Loads a single <see cref="Observation"/> with its <c>Author</c>,
    /// <c>Taxon</c>, <c>Comments</c> (each with <c>Author</c>), and
    /// <c>Proposals</c> (each with <c>Author</c> + <c>Taxon</c>) eagerly
    /// loaded. Returns null when no observation has that id.
    /// </summary>
    Observation? GetObservationWithAttachments(int id);

    List<CommentDTO> GetCommentsForObservation(int observationId);
    List<ProposalDTO> GetProposalsForObservation(int observationId);
}
