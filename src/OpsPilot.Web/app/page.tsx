"use client";

import { FormEvent, useState } from "react";

type Analysis = {
  summary: string;
  probableCause: string;
  recommendedAction: string;
  confidence: number;
  sources: string[];
  evidence: string[];
  toolsUsed: string[];
};

type ProblemDetails = {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
};

export default function Home() {
  const [serviceName, setServiceName] = useState("payments");
  const [issue, setIssue] = useState("503 errors after deployment");
  const [result, setResult] = useState<Analysis | null>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function analyze(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError("");
    setResult(null);
    try {
      const response = await fetch("/api/troubleshooting/analyze", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ serviceName, issue }),
      });
      if (!response.ok) {
        const problem = (await response.json()) as ProblemDetails;
        const validation = problem.errors ? Object.values(problem.errors).flat().join(" ") : "";
        throw new Error(validation || problem.detail || problem.title ||
          "Analysis failed with status " + response.status + ".");
      }
      setResult((await response.json()) as Analysis);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message :
        "The analysis could not be completed.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main>
      <header className="masthead">
        <a className="brand" href="/" aria-label="OpsPilot home">
          <span className="brandMark">OP</span><span>OpsPilot</span>
        </a>
        <span className="systemStatus"><span aria-hidden="true" />Technical troubleshooting agent</span>
      </header>

      <section className="hero">
        <p className="eyebrow">RAG + MCP WORKFLOW</p>
        <h1>Turn an incident into an evidence-backed next step.</h1>
        <p className="intro">Describe a service issue. OpsPilot searches technical runbooks,
          checks simulated telemetry, and returns a structured diagnosis.</p>
      </section>

      <section className="workspace">
        <form className="analysisForm" onSubmit={analyze}>
          <div className="formHeading">
            <div><p className="step">01 / INCIDENT</p><h2>Analyze an issue</h2></div>
            <span className="localBadge">LOCAL DATA</span>
          </div>
          <label htmlFor="serviceName">Service name</label>
          <input id="serviceName" value={serviceName}
            onChange={(event) => setServiceName(event.target.value)}
            placeholder="payments" disabled={loading} required />
          <label htmlFor="issue">Issue</label>
          <textarea id="issue" value={issue}
            onChange={(event) => setIssue(event.target.value)}
            placeholder="503 errors after deployment" rows={6} disabled={loading} required />
          <button type="submit" disabled={loading}>
            {loading ? <><span className="spinner" aria-hidden="true" />Analyzing evidence</> :
              <>Analyze incident<span aria-hidden="true">→</span></>}
          </button>
          {error && <div className="error" role="alert">
            <strong>Analysis unavailable</strong><span>{error}</span>
          </div>}
        </form>

        <section className={"resultPanel " + (result ? "hasResult" : "")} aria-live="polite">
          {!result && !loading && <EmptyState step="02 / DIAGNOSIS"
            title="Evidence will appear here"
            text="Run an analysis to inspect the diagnosis, sources, tools, and telemetry." />}
          {loading && <EmptyState step="SEARCHING" title="Correlating signals"
            text="Searching runbooks and consulting technical tools." active />}
          {result && <article className="diagnosis">
            <div className="resultTopline">
              <p className="step">02 / DIAGNOSIS</p>
              <div className="confidence"><span>CONFIDENCE</span>
                <strong>{Math.round(result.confidence * 100)}%</strong></div>
            </div>
            <section className="summary"><h2>{result.summary}</h2></section>
            <ResultBlock number="01" title="Probable cause"><p>{result.probableCause}</p></ResultBlock>
            <ResultBlock number="02" title="Recommended action"><p>{result.recommendedAction}</p></ResultBlock>
            <div className="metadataGrid">
              <ResultList title="Runbook sources" items={result.sources} empty="No runbooks matched." />
              <ResultList title="Tools used" items={result.toolsUsed} empty="No tools used." mono />
            </div>
            <ResultBlock number="03" title="Evidence">
              {result.evidence.length ? <ul className="evidenceList">
                {result.evidence.map((item, index) => <li key={item + "-" + index}>{item}</li>)}
              </ul> : <p>No evidence was found.</p>}
            </ResultBlock>
          </article>}
        </section>
      </section>
      <footer><span>OpsPilot / Portfolio system</span><span>.NET 8 · Next.js · Qdrant · MCP</span></footer>
    </main>
  );
}

function EmptyState({ step, title, text, active = false }:
  { step: string; title: string; text: string; active?: boolean }) {
  return <div className="emptyState">
    <div className={"radar " + (active ? "active" : "")} aria-hidden="true"><span /></div>
    <p className="step">{step}</p><h2>{title}</h2><p>{text}</p>
  </div>;
}

function ResultBlock({ number, title, children }:
  { number: string; title: string; children: React.ReactNode }) {
  return <section className="resultBlock">
    <div className="blockTitle"><span>{number}</span><h3>{title}</h3></div>{children}
  </section>;
}

function ResultList({ title, items, empty, mono = false }:
  { title: string; items: string[]; empty: string; mono?: boolean }) {
  return <section className="metaBlock"><h3>{title}</h3>
    {items.length ? <ul className={mono ? "tagList mono" : "tagList"}>
      {items.map((item) => <li key={item}>{item}</li>)}
    </ul> : <p>{empty}</p>}
  </section>;
}
