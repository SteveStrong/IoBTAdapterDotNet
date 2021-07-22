/*-------------------------------------------------------------------------------------------------
| *** UNCLASSIFIED ***
|--------------------------------------------------------------------------------------------------
| U.S. Army Research, Development, and Engineering Command
| Aviation and Missile Research, Development, and Engineering Center
| Software Engineering Directorate, Redstone Arsenal, AL
|--------------------------------------------------------------------------------------------------
| Export-Control Act Warning: Warning - This Document contains technical data whose export is
| restricted by the Arms Export Control Act (Title 22, U.S.C., Sec 2751, et seq) or the Export
| Administration Act of 1979, as amended, Title 50, U.S.C., App. 2401 et seq. Violations of these
| export laws are subject to severe criminal penalties. Disseminate in accordance with provisions
| of DoD Directive 5230.25.
|--------------------------------------------------------------------------------------------------
| Distribution Statement C:  Distribution authorized to U.S. Government Agencies and their
| contractors, Export Controlled, Critical Technology, 13 Feb 2017. Other request for this document
| shall be referred to U.S. Army Aviation and Missile Research Development and Engineering Center
| Software Engineering Directorate, Attn: RDMR-BAW, Redstone Arsenal, AL 35898.
--------------------------------------------------------------------------------------------------*/

package mil.airforce.mc2.sdk.common.proxy;

import com.google.protobuf.Empty;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.Executors;
import java.util.function.Consumer;

import io.grpc.StatusRuntimeException;
import mil.airforce.mc2.logging.IDonLogging;
import mil.airforce.mc2.logging.LoggingFactory;
import mil.airforce.mc2.rx.IRxAction;
import mil.airforce.mc2.rx.IRxDisposable;
import mil.airforce.mc2.sdk.common.ClientCredentials;
import mil.airforce.mc2.sdk.generated.CommonProto;
import mil.airforce.mc2.sdk.generated.ThreatsManagerGrpc;
import mil.airforce.mc2.sdk.generated.ThreatsManagerGrpc.ThreatsManagerBlockingStub;
import mil.airforce.mc2.sdk.generated.ThreatsManagerGrpc.ThreatsManagerStub;
import mil.airforce.mc2.sdk.generated.ThreatsManagerProto.ThreatAffiliationInfo;
import mil.airforce.mc2.sdk.generated.ThreatsManagerProto.ThreatsManagerSubscription;
import mil.airforce.mc2.sdk.generated.ThreatsManagerProto.ThreatsManagerSubscriptionResponse;
import mil.airforce.mc2.sdk.generated.ThreatsProto.ThreatInfo;

/**
 * Wrapper class for calling into the threats manager. Support for blocking and async calls.
 */
public class ThreatsManagerProxy extends
    ObservableServiceProxy<ThreatsManagerSubscription, ThreatsManagerSubscriptionResponse>
    implements IThreatsManagerProxy
{
  private static final IDonLogging logger = LoggingFactory.getLogger(ThreatsManagerProxy.class);

  private ThreatsManagerBlockingStub blocking;
  private ThreatsManagerStub async;

  /**
   * Constructor for our client proxy into the threats manager service.
   *
   * @param host        The IP address for where the service is running
   * @param port        The port on which the service is running
   * @param credentials client information for all calls going to the server
   */
  public ThreatsManagerProxy(String host, int port, ClientCredentials credentials)
  {
    super(host, port, credentials);
    init();
  }

  /**
   * Initializes class after dependencies have been injected
   */
  @Override
  protected void init()
  {
    super.init();

    blocking = ThreatsManagerGrpc.newBlockingStub(channel).withCallCredentials(credentials);
    async = ThreatsManagerGrpc.newStub(channel).withCallCredentials(credentials);
  }

  /**
   * Get the version information for the threats manager, or null if we cannot connect to the threats manager.
   *
   * @return version detail information
   */
  @Override
  public CommonProto.VersionInfo getVersion()
  {
    if (blocking == null)
    {
      logger.e("getVersion:: error in threats manager initialization. Operation cancelled");
      return null;
    }

    try
    {
      return blocking.getVersion(Empty.getDefaultInstance());
    }
    catch (StatusRuntimeException ex)
    {
      logger.e("getVersion:: Cannot connect to SDK: " + ex.getMessage());
      return null;
    }
  }

  /**
   * Subscribe to either threats from the medusa correlation application or affiliations changes. stream without a
   * definitive end to the set. The client will continue to receive ThreatsManagerSubscription objects until the request
   * is made to stop or the server is shut down.
   *
   * @param subscription the subscription details for this client
   * @param onNext       the action to call as new responses are received
   * @param onError      the action to call if an error is thrown during event processing
   * @param onComplete   the action to call when the stream is completed
   * @return the client token for unsubscribing from the event stream of responses
   */
  @Override
  public CompletableFuture<IRxDisposable> subscribe(
      ThreatsManagerSubscription subscription,
      Consumer<ThreatsManagerSubscriptionResponse> onNext,
      Consumer<? super Throwable> onError,
      IRxAction onComplete)
  {
    CompletableFuture<IRxDisposable> future = new CompletableFuture<>();

    // the threaded code we are running
    Runnable task = () ->
    {
      if (blocking == null || async == null)
      {
        logger.e("subscribe:: error in threats manager initialization. Operation cancelled");
        future.cancel(false);
        return;
      }

      if (responseObserver != null)
      {
        logger.w("Client is already subscribed. Disposing of previous connection");
        this.completeObserver();
      }

      blockUntilConnected(future);

      responseObserver = new StreamObserverToRxObserver<>();
      IRxDisposable dspsbl = responseObserver.getSubject().observable().subscribe(
          onNext, onError, onComplete,
          (IRxDisposable d) ->
          {
            // we get here when the client calls dispose on the provided token.
            // when this happens, call into the server to cancel the subscription.
            logger.i("received threat subscription dispose from client: " + d);

            try
            {
              this.blocking.cancelSubscription(Empty.getDefaultInstance());
            }
            catch (StatusRuntimeException ex)
            {
              logger.w("unsubscribe exception - threats service not available: " + ex.getMessage());
            }

            this.completeObserver();
          });

      try
      {
        logger.i("Subscribing to threats stream from server for: " + dspsbl);
        this.async.subscribe(subscription, responseObserver);
      }
      catch (StatusRuntimeException ex)
      {
        logger.w("subscription exception - threats service not available: " + ex.getMessage());
        future.cancel(false);
        responseObserver.getSubject().observer().onError(ex);
      }

      // completes the future request, passing the final result back to the client
      future.complete(dspsbl);
    };

    // run it
    Executors.newCachedThreadPool().submit(task);

    // immediately return the future object
    return future;
  }

  /**
   * The blocking call to verify that the service is up and available
   */
  @Override
  void makeBlockingConnectionCall()
  {
    blocking.getVersion(Empty.getDefaultInstance());
  }

  /**
   * Add a complete client-driven threat into the DON system.
   *
   * @param threatInfo the threat to add
   * @return {@code true} if the treat was successfully added to DON; otherwise, {@code false}
   */
  @Override
  public boolean addThreat(ThreatInfo threatInfo)
  {
    if (blocking == null)
    {
      logger.e("addThreat:: error in threats manager initialization. Operation cancelled");
      return false;
    }

    logger.t("adding threat " + threatInfo.getThreatId() + " to system");

    try
    {
      CommonProto.ResponseInfo response = this.blocking.addThreat(threatInfo);
      logger.t(response.getDetails());
      return response.getSuccess();
    }
    catch (StatusRuntimeException ex)
    {
      logger.e("addThreat:: Cannot connect to SDK: " + ex.getMessage());
      return false;
    }
  }

  /**
   * Allow a client to affiliate a threat given its threat ID.
   *
   * @param affiliationInfo the threat affiliation information
   * @return true if the affiliation request was successfully sent to DON; otherwise false
   */
  @Override
  public CommonProto.ResponseInfo setAffiliation(ThreatAffiliationInfo affiliationInfo)
  {
    CommonProto.ResponseInfo response;
    String msg;
    if (blocking == null)
    {
      msg = "setAffiliation:: error in threats manager initialization. Operation cancelled";
      logger.e(msg);
      response = CommonProto.ResponseInfo.newBuilder()
          .setDetails(msg)
          .setSuccess(false)
          .build();
      return response;
    }

    logger.t("Affiliating threat " + affiliationInfo.getThreatId() + " to " + affiliationInfo.getAffiliation());

    try
    {
      response = this.blocking.setAffiliation(affiliationInfo);
      logger.t(response.getDetails());
      return response;
    }
    catch (StatusRuntimeException ex)
    {
      msg = "setAffiliation:: Cannot connect to SDK";
      logger.e(msg + ": " + ex.getMessage());
      response = CommonProto.ResponseInfo.newBuilder()
          .setDetails(msg)
          .setSuccess(false)
          .build();
      return response;
    }
  }

  /**
   * Allow a client to get the list of affiliations changes
   *
   * @return a List of ThreatAffiliationInfo
   */
  @Override
  public List<ThreatAffiliationInfo> getAffiliations()
  {
    List<ThreatAffiliationInfo> list = new ArrayList<>();
    if (blocking == null)
    {
      logger.e("getAffiliations:: error in threats manager initialization. Operation cancelled");
      return null;
    }

    Iterator<ThreatAffiliationInfo> iter = this.blocking.getAffiliations(Empty.getDefaultInstance());
    iter.forEachRemaining(list::add);
    return list;
  }

  @Override
  public String getName()
  {
    return "Medusa Threats Manager proxy";
  }
}
